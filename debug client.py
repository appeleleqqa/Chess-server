import socket


HOST = '127.0.0.1'  # Standard loopback interface address (localhost)
PORT = 5002        # Port to listen on (non-privileged ports are > 1023)

print("started")
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((HOST, PORT))
sock.sendall(b'Hello, world')
data = s.recv(1024)

print('Received', repr(data))
        