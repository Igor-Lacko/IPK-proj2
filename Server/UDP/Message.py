# Contains all messages which can be sent by the client

class AuthMessage:
    def __init__(self, messageID, username, displayName, secret):
        self.MessageID = messageID
        self.Username = username
        self.DisplayName = displayName
        self.Secret = secret

    def Parse(message):
        messageID = int.from_bytes(message[1:3], 'big')

        """This horrifying code is valid since i won't send invalid messages from the client"""
        username = ""
        displayName = ""
        secret = ""

        # username
        for i in range(3, len(message)):
            if message[i] == 0:
                username = message[3:i].decode('utf-8')

                # displayname
                for j in range(i + 1, len(message)):
                    if(message[j] == 0):
                        displayName = message[i + 1:j].decode('utf-8')

                        # secret
                        for k in range(j + 1, len(message)):
                            if(message[k] == 0):
                                secret = message[j + 1:k].decode('utf-8')
                                return AuthMessage(messageID, username, displayName, secret)

    def PrintMessage(self):
        print("TYPE: AUTH")
        print(f"ID: {self.MessageID}")
        print(f"USERNAME: {self.Username}")
        print(f"DISPLAYNAME: {self.DisplayName}")
        print(f"SECRET: {self.Secret}")

class ConfirmMessage:
    def __init__(self, refMessageID):
        self.RefMessageID = refMessageID

    def Parse(message):
        """Nothing more to do here..."""
        messageID = int.from_bytes(message[1:3], 'big')
        return ConfirmMessage(messageID)

    def PrintMessage(self):
        print("TYPE: CONFIRM")
        print(f"REFID: {self.RefMessageID}")

class JoinMessage:
    def __init__(self, messageID, channelID, displayName):
        self.MessageID = messageID
        self.ChannelID = channelID
        self.DisplayName = displayName

    def Parse(message):
        """Class method, returns a JoinMessage object"""
        messageID = int.from_bytes(message[1:3], 'big')

        # displayname
        for i in range(3, len(message)):
            if message[i] == 0:
                channelID = message[3:i].decode('utf-8')
                for j in range(i + 1, len(message)):
                    if message[j] == 0:
                        displayName = message[i + 1:j].decode('utf-8')
                        return JoinMessage(messageID, channelID, displayName)

    def PrintMessage(self):
        print("TYPE: JOIN")
        print(f"ID: {self.MessageID}")
        print(f"DISPLAYNAME: {self.DisplayName}")

class MsgMessage:
    def __init__(self, messageID, displayName, content):
        self.MessageID = messageID
        self.DisplayName = displayName
        self.Content = content

    def Parse(message):
        """Class method, returns a MsgMessage object"""
        messageID = int.from_bytes(message[1:3], 'big')
        displayName = ""
        content = ""

        # displayname
        for i in range(3, len(message)):
            if message[i] == 0:
                displayName = message[3:i].decode('utf-8')

                # content
                for j in range(i + 1, len(message)):
                    if(message[j] == 0):
                        content = message[i + 1:j].decode('utf-8')
                        return MsgMessage(messageID, displayName, content)

    def PrintMessage(self):
        print("TYPE: MSG")
        print(f"ID: {self.MessageID}")
        print(f"DISPLAYNAME: {self.DisplayName}")
        print(f"CONTENT: {self.Content}")

class ErrMessage:
    def __init__(self, messageID, displayName, content):
        self.MessageID = messageID
        self.DisplayName = displayName
        self.Content = content

    def Parse(message):
        """Class method, returns a ErrMessage object"""
        messageID = int.from_bytes(message[1:3], 'big')
        displayName = ""
        content = ""

        # displayname
        for i in range(3, len(message)):
            if message[i] == 0:
                displayName = message[3:i].decode('utf-8')

                # content
                for j in range(i + 1, len(message)):
                    if(message[j] == 0):
                        content = message[i + 1:j].decode('utf-8')
                        return ErrMessage(messageID, displayName, content)

    def PrintMessage(self):
        print("TYPE: ERR")
        print(f"ID: {self.MessageID}")
        print(f"DISPLAYNAME: {self.DisplayName}")
        print(f"CONTENT: {self.Content}")

class ByeMessage:
    def __init__(self, messageID, displayName):
        self.MessageID = messageID
        self.DisplayName = displayName

    def Parse(message):
        """Class method, returns a ByeMessage object"""
        messageID = int.from_bytes(message[1:3], 'big')

        # displayname
        for i in range(3, len(message)):
            if message[i] == 0:
                displayName = message[3:i].decode('utf-8')
                return ByeMessage(messageID, displayName)

    def PrintMessage(self):
        print("TYPE: BYE")
        print(f"ID: {self.MessageID}")
        print(f"DISPLAYNAME: {self.DisplayName}")