import socket

class UDP_client(object):

    def __init__(self, ip, port):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.ip = ip
        self.port = port
        return

    def send_msg(self, msg):
        self.sock.sendto(msg, (self.ip, self.port))
        return
        
    def close(self):
        self.sock.close()
        return
