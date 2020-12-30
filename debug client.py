import json
import socket

HOST = '127.0.0.1'  # Standard loopback interface address (localhost)
PORT = 5002        # Port to listen on (non-privileged ports are > 1023)

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((HOST, PORT))
user = {}
user["Code"] = int(input("do you have a user(1 = yes, 2 = no): "))
user["Username"] = input("username: ")
user["Password"] = input("password: ")
sock.sendall(json.dumps(user).encode())
data = sock.recv(1024)

print('Received', repr(data))
        
