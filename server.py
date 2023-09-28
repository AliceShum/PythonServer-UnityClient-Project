# — coding: utf-8 –
#yolo相关
import math
from glob import glob
import cv2
import numpy as np
import json

from torch import nn
from ultralytics import YOLO
import matplotlib.pyplot as plt
import torch

#socket相关的
import socket
import threading
import struct
import time

import base64

HOST = ''
PORT = 10086

headerSize = 4 #指示包有多大
notContentSize = 12 #非内容的大小

model = YOLO('back_model/back.pt') 

#检查、修改权重信息，并计算目标图片的标记点
#TODO 具体要根据后面选择的身体部位修改，暂时只有一个权重
def checkWeight(): #(checkWeight)
    '''new_weight = None
    if ctype == 1:
        new_weight = 'model'
    else:
        new_weight = 'default'

    print(str(ctype) + "  model weight is :" + new_weight + " -- " + str(savedOpt.load_weights) ) '''   

    calculatePic()

def process_frame():
    img_path = "E:\\yolo_test\\a.jpg"
    res = model(img_path)
    
    data = res[0].keypoints.xy[0][:84]
    #print("=========> " + str(data))

    # 将data转换为Tensor对象
    data_tensor = torch.tensor(data).clone().detach()
    # 将Tensor对象转换为列表
    data_list = data_tensor.tolist()
    # 转换为指定格式的字典列表
    arr = [{"X": item[0], "Y": item[1]} for item in data_list]
    # 创建包含arr的字典
    result = {"arr": arr}
    #print("result =========> " + str(result))
    return result
        
    # 把点的json数据保存为JSON文件
    '''with open("E:\\yolo_test\\result\\points.txt", 'w', encoding='utf-8') as json_file:
        json_data = json.dumps(result, ensure_ascii=False)
        json_file.write(json_data)
    #with open("back_img/points.txt", "w") as json_file:
    #    json.dump(result, json_file)

    #把点生长在原图上，并保存
    # 读取PNG图片
    img = cv2.imread(img_path) 
    # 定义点的颜色和半径
    color = (0, 0, 255)  # 红色
    radius = 5
    # 循环遍历数组中的每个点，并在图像上绘制它
    for point in data_list:
        x, y = point
        #print(str(x) + "--" + str(y))
        cv2.circle(img, (int(x), int(y)), radius, color, -1)
    # 保存绘制后的图像
    cv2.imwrite('back_img/output.jpg', img)'''

#训练目标图片
def calculatePic():
    try:
        result = process_frame()
        print("train single picture OK!")
        sendResultToClient(True, result)

    except Exception as e:
        print("=========ERROR========== " + str(e.args))
        sendResultToClient(False, "yolo.error")

#下面4个方法是socket用
#按照客户端解析的数据格式准备数据
#4 int(该包完整的大小), 2 short(stype 服务号), 2 short(ctype), 4 empty (占位，没用到), content(具体内容的字节)    
def getBytesPack(stype, ctype, array_data):   
    arr1 = stype.to_bytes(2, 'little')
    arr2 = ctype.to_bytes(2, 'little')
    arr0 = bytes(4)
    arr3 = arr1 + arr2 + arr0 + array_data
    length = len(arr3) + 4
    arr4 = length.to_bytes(4, 'little')
    arr5 = arr4 + arr3
    return arr5

#读取接收到的字节流包有多大(包头、stype、ctype、内容长度的总和)
def readPackageSize(buffer):
    head_buffer = buffer[0:4]
    pkg_size = int.from_bytes(head_buffer, byteorder='little',signed=False)
    return pkg_size

#解析socket收到的数据
def dataHandle(bytes_data):
    #本来应该判断stype和ctype，这里不判断了
    '''head_buffer = bytes_data[0:4]
    pkg_size = int.from_bytes(head_buffer, byteorder='little',signed=False)
    stype_buffer = bytes_data[4:6]
    stype = int.from_bytes(stype_buffer, byteorder='little',signed=False)
    ctype_buffer = bytes_data[6:8]
    ctype = int.from_bytes(ctype_buffer, byteorder='little',signed=False)
    print("解析socket收到的数据(包的长度、stype、ctype):" + str(pkg_size) + "   " + str(stype) + "  " + str(ctype))'''
    
    content_buffer = bytes_data[8:]
    tu_b = base64.b64decode(content_buffer)  #这样是不解析stype ctype了，直接把图片保存到本地
    with open('a.jpg', 'wb') as fp:
        fp.write(tu_b)
        return True
    return False

#发送结果到客户端
def sendResultToClient(isSuccess, result):
    stype = 5 #客户端接收该消息的stype是哪一个
    ctype = 2 #成功：2 失败：4   客户端接收该消息的ctype是哪一个
    if isSuccess == False:
        ctype = 4
    data = str(result) 
    data = data.replace('\'','\"') #把json里面的''改成""，不然unity解析会报错
    send_pack = getBytesPack(stype, ctype, data.encode('utf-8'))
    conn.send(send_pack)

#开启socket监听,接收消息
def startServer():
    dataBuffer = bytes() #多次接收到的字节流数据
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((HOST, PORT))
        s.listen(1) #监听的客户端个数
        global conn
        conn, addr = s.accept()
        with conn:
            print('客户端连接：', addr)
            while True:
                data = conn.recv(1024) #接收的字节个数
                if data:
                    # 把数据存入缓冲区，类似于push数据
                    dataBuffer += data
                    while True:
                        if len(dataBuffer) < headerSize:
                            #print("数据包（%s Byte）小于消息头部长度，跳出小循环" % len(dataBuffer))
                            break

                        # 读取包的完整大小
                        bodySize = readPackageSize(dataBuffer)

                        # 分包情况处理，跳出函数继续接收数据
                        if len(dataBuffer) < bodySize :
                            print("数据包（%s Byte）不完整（总共%s Byte），跳出小循环" % (len(dataBuffer), headerSize+bodySize))
                            break
                        # 读取消息正文的内容
                        body = dataBuffer[0:bodySize]

                        # 数据处理(包头、stype、ctype、内容都在里面)
                        result = dataHandle(body)
                        if result:
                            checkWeight() 

                        # 粘包情况的处理
                        dataBuffer = dataBuffer[bodySize:] # 获取下一个数据包，类似于把数据pop出

#测试用，可以删（打印当前时间、时间戳）
def print_time_stamp():
    ct = time.time()
    local_time = time.localtime(ct)
    data_head = time.strftime("%Y-%m-%d %H:%M:%S", local_time)
    data_secs = (ct - int(ct)) * 1000
    time_stamp = "%s.%03d" % (data_head, data_secs)
    print(time_stamp)
    #stamp = ("".join(time_stamp.split()[0].split("-"))+"".join(time_stamp.split()[1].split(":"))).replace('.', '')
    #print(stamp)
    
if __name__ == '__main__':
    startServer()