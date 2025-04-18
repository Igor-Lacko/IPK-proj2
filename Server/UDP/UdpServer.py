# This script acts as a pseudo UDP server for IPK25CHAT, using user comands to control the messages sent.
# Generated with the help of ChatGPT

import threading
import socket
from Message import *

CONFIRM = 0
REPLY = 1
AUTH = 2
JOIN = 3
MSG = 4
PING = 253
ERR = 254
BYE = 255

MAX_MTU = 1500

class UdpServer:
    def __init__(self, port = 4567):
        self.Port = port
        self.MessageID = 0
        self.ReceivedLastID : int = None
        self.InitialSocket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.InitialSocket.bind(("localhost", self.Port))
        self.DynamicSocket = None
        self.ClientAddress = None
        self.IsFinished = False

    def Start(self):
        """Starts the server"""
        # threads
        (clientThread := threading.Thread(target=self.ClientLoop, daemon=True)).start()
        (inputThread := threading.Thread(target=self.InputLoop, daemon=True)).start()

        # wait for threads to finish
        clientThread.join()
        inputThread.join()

    def Close(self):
        """At the end"""
        self.InitialSocket.close()
        if self.DynamicSocket is not None:
            self.DynamicSocket.close()

    def ClientLoop(self):
        """Receive and print messages"""
        while not self.IsFinished:
            # not yet allocated a dynamic port for the client
            if self.DynamicSocket is None:
                # only auth will come in anyway
                data, addr = self.InitialSocket.recvfrom(MAX_MTU)

                # set the client address to later sendto
                self.ClientAddress = addr

                # handle the message
                (message := self.GetIncomingMessage(data)).PrintMessage()
                if not isinstance(message, ConfirmMessage):
                    self.ReceivedLastID = message.MessageID
                    self.SendConfirm(message.MessageID)
                    # allocate a new socket
                    self.DynamicSocket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
                    self.DynamicSocket.bind(("localhost", 0)) # any port

            else:
                data, addr = self.DynamicSocket.recvfrom(MAX_MTU)

                # filter out traffic (there probably shoultn't be any on localhost, but still)
                if addr != self.ClientAddress:
                    continue

                # message from client, handle
                (message := self.GetIncomingMessage(data)).PrintMessage()
                if not isinstance(message, ConfirmMessage):
                    self.ReceivedLastID = message.MessageID
                    self.SendConfirm(message.MessageID)

    def InputLoop(self):
        """read commands from stdin and send messages to the client"""
        while not self.IsFinished:
            message = self.ParseMessageFromString(input())
            if message is None:
                self.IsFinished = True
                self.Close()
                break

            # send from either socket
            if self.DynamicSocket is None and self.ClientAddress is not None: # probably won't ever happen
                self.InitialSocket.sendto(message, self.ClientAddress)

            else:
                self.DynamicSocket.sendto(message, self.ClientAddress)

    def ParseMessageFromString(self, message : str):
        """Returns a message based on a simplified pattern (first word)"""
        split = message.split(" ")
        match(split[0]):
            case "reply":
                self.MessageID += 1
                content = " ".join(split[1:]).encode('ascii')
                return REPLY.to_bytes(1, 'big') + (self.MessageID - 1).to_bytes(2, 'big') + int(1).to_bytes(1, 'big') + self.ReceivedLastID.to_bytes(2, 'big') + content + b'\x00'

            case "reply!":
                self.MessageID += 1
                content = " ".join(split[1:]).encode('ascii')
                return REPLY.to_bytes(1, 'big') + (self.MessageID - 1).to_bytes(2, 'big') + int(0).to_bytes(1, 'big') + self.ReceivedLastID.to_bytes(2, 'big') + content + b'\x00'

            case "msg":
                self.MessageID += 1
                content = " ".join(split[1:]).encode('ascii')
                return MSG.to_bytes(1, 'big') + (self.MessageID - 1).to_bytes(2, 'big') + b"server" + b'\x00' + content + b'\x00'

            case "err":
                self.MessageID += 1
                content = " ".join(split[1:]).encode('ascii')
                return ERR.to_bytes(1, 'big') + (self.MessageID - 1).to_bytes(2, 'big') + b"server" + b'\x00' + content + b'\x00'

            case "bye":
                self.MessageID += 1
                return BYE.to_bytes(1, 'big') + (self.MessageID - 1).to_bytes(2, 'big') + b"server" + b'\x00'

            case "ping":
                self.MessageID += 1
                return PING.to_bytes(1, 'big') + (self.MessageID - 1).to_bytes(2, 'big')
            
            case "malformed":
                return message.encode('ascii')
            
            case _:
                return None

    def GetIncomingMessage(self, message : bytes):
        type = message[0]
        if type == CONFIRM:
            return ConfirmMessage.Parse(message)

        elif type == AUTH:
            return AuthMessage.Parse(message)

        elif type == JOIN:
            return JoinMessage.Parse(message)

        elif type == MSG:
            return MsgMessage.Parse(message)

        elif type == ERR:
            return ErrMessage.Parse(message)

        elif type == BYE:
            return ByeMessage.Parse(message)

    def SendConfirm(self, messageID : int):
        if self.DynamicSocket is None:
            self.InitialSocket.sendto(CONFIRM.to_bytes(1, 'big') + messageID.to_bytes(2, 'big'), self.ClientAddress)

        else:
            self.DynamicSocket.sendto(CONFIRM.to_bytes(1, 'big') + messageID.to_bytes(2, 'big'), self.ClientAddress)

def main():
    server = UdpServer()
    server.Start()

if __name__ == "__main__":
    main()