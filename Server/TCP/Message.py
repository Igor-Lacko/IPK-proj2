# Contains all messages which can be sent by the client

class AuthMessage:
    def __init__(self, username, displayName, secret):
        self.Username = username
        self.DisplayName = displayName
        self.Secret = secret

    def Parse(message):
        """AUTH ID AS DNAME USING SECRET"""
        split = message.split(" ")
        username = split[1]
        displayName = split[3]
        secret = split[5]
        return AuthMessage(username, displayName, secret)


    def PrintMessage(self):
        print("TYPE: AUTH")
        print(f"USERNAME: {self.Username}")
        print(f"DISPLAYNAME: {self.DisplayName}")
        print(f"SECRET: {self.Secret}")

class JoinMessage:
    def __init__(self, channelID, displayName):
        self.ChannelID = channelID
        self.DisplayName = displayName

    def Parse(message):
        """JOIN ID AS DNAME"""
        split = message.split(" ")
        channelID = split[1]
        displayName = split[3]
        return JoinMessage(channelID, displayName)

    def PrintMessage(self):
        print("TYPE: JOIN")
        print(f"DISPLAYNAME: {self.DisplayName}")

class MsgMessage:
    def __init__(self, displayName, content):
        self.DisplayName = displayName
        self.Content = content

    def Parse(message):
        """MSG FROM DNAME IS CONTENT"""
        split = message.split(" ")
        displayName = split[2]
        content = split[4]
        return MsgMessage(displayName, content)

    def PrintMessage(self):
        print("TYPE: MSG")
        print(f"DISPLAYNAME: {self.DisplayName}")
        print(f"CONTENT: {self.Content}")

class ErrMessage:
    def __init__(self, displayName, content):
        self.DisplayName = displayName
        self.Content = content

    def Parse(message):
        """ERR FROM DNAME IS CONTENT"""
        split = message.split(" ")
        displayName = split[2]
        content = split[4]
        return ErrMessage(displayName, content)

    def PrintMessage(self):
        print("TYPE: ERR")
        print(f"DISPLAYNAME: {self.DisplayName}")
        print(f"CONTENT: {self.Content}")

class ByeMessage:
    def __init__(self, displayName):
        self.DisplayName = displayName

    def Parse(message):
        """BYE FROM DNAME"""
        split = message.split(" ")
        displayName = split[2]
        return ByeMessage(displayName)

    def PrintMessage(self):
        print("TYPE: BYE")
        print(f"DISPLAYNAME: {self.DisplayName}")