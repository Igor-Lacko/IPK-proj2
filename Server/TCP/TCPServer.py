# This script acts as a pseudo TCP server for IPK25CHAT, using user comands to control the messages sent.
# Generated with the help of ChatGPT

import threading
import socket
from Message import *

class TCPServer:
    def __init__(self, port=4567):
        self.Port = port
        self.Socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.Socket.bind(("localhost", self.Port))
        self.Socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.Socket.listen(1)
        self.IsFinished = False

    def Start(self):
        """Starts the server"""
        self.ClientSocket = self.Socket.accept()[0]

        # threads
        (clientThread := threading.Thread(target=self.ClientLoop, daemon=True)).start()
        (inputThread := threading.Thread(target=self.InputLoop, daemon=True)).start()

        # wait for threads to finish
        clientThread.join()
        inputThread.join()

    def Close(self):
        """Closes the server"""
        try:
            self.ClientSocket.shutdown(socket.SHUT_RDWR)
            self.ClientSocket.close()
        except OSError:
            pass

        try:
            self.Socket.shutdown(socket.SHUT_RDWR)
            self.Socket.close()
        except OSError:
            pass

    def ClientLoop(self):
        """Receive and print messages"""
        while not self.IsFinished:
            # this is safe, since i won't send another message behind the first in a datagram
            unprocessed = ""
            while True:
                part = self.ClientSocket.recv(1024).decode("ascii")

                # for example client closed the connection (should be an err nonetheless?)
                if not part:
                    self.IsFinished = True
                    self.Close()
                    return
                unprocessed += part
                if "\r\n" in part:
                    break

            message = self.GetIncomingMessage(unprocessed)
            message.PrintMessage()

            # client closed connection
            if(isinstance(message, ByeMessage)) or (isinstance(message, ErrMessage)):
                self.IsFinished = True
                self.Close()
                break


    def InputLoop(self):
        """Read commands from stdin and send messages to the client"""
        while not self.IsFinished:
            message = self.ParseMessageFromString(input("Type in message to send to the client or press enter to exit:\n"))
            if self.IsFinished:
                break

            if message is None:
                self.IsFinished = True
                self.Close()

            else:
                self.ClientSocket.send(message.encode("ascii"))

    def ParseMessageFromString(self, message: str):
        """Returns a message based on a simplified pattern (first word)"""
        split = message.split(" ")
        match(split[0]):
            case "reply":
                content = " ".join(split[1:])
                return f"REPLY OK IS {content}\r\n"

            case "reply!":
                content = " ".join(split[1:])
                return f"REPLY NOK IS {content}\r\n"

            case "msg":
                content = " ".join(split[1:])
                return f"MSG FROM SERVER IS {content}\r\n"

            case "msgstart":
                content = " ".join(split[1:])
                return f"MSG FROM SERVER IS {content}"

            case "msgpart":
                content = " " + " ".join(split[1:])
                return f"{content}"

            case "msgend":
                content = " " + " ".join(split[1:])
                return f"{content}\r\n"

            case "msgmultiple":
                contents = "".join([f"MSG FROM SERVER IS {word}\r\n" for word in split[1:]])
                return contents

            case "bye":
                return "BYE FROM SERVER\r\n"

            case "err":
                content = " ".join(split[1:])
                return f"ERR FROM SERVER IS {content}\r\n"

            case "malformed":
                content = " ".join(split[1:])
                return f"{content}\r\n"
            
            case _:
                return None

    def GetIncomingMessage(self, message : str):
        type = message.split(" ")[0]
        if type == "AUTH":
            return AuthMessage.Parse(message)

        elif type == "JOIN":
            return JoinMessage.Parse(message)

        elif type == "MSG":
            return MsgMessage.Parse(message)

        elif type == "ERR":
            return ErrMessage.Parse(message)

        elif type == "BYE":
            return ByeMessage.Parse(message)

def main():
    server = TCPServer()
    server.Start()

if __name__ == "__main__":
    main()