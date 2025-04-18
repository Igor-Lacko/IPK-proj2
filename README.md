# Client for the IPK25-CHAT protocol

## Introduction
This program implements a client application for a chat server using the IPK25-CHAT protocol.
It has two variants, both using a different transport layer protocol (TCP or UDP) to communifcate with the server.
It was implemented as a part of the IPK (Computer Communications and Networks) course on the Brno University of Technology,
Faculty of Information Technology.

## Table of contents
todo

## Theoretical overview

### Transmission control protocol (TCP)
The TCP is a transport layer protocol which is well characterized by reliable delivery
of data (being able to solve problems such as packet loss), being connection-oriented, and providing a in-order byte stream [[RFC9293]](#bibliography). 
Because of these features, the TCP is commonly utilized in applications that require reliable data transfer, web browsing, email or file transfer [[Techtarget]](#bibliography). 
However, the reliablility guarantees can introduce significant overhead, which is why the TCP isn't suitable for all use cases. The TCP is most recently specified in [[RFC9293]](#bibliography).
***
### User datagram protocol (UDP)
The UDP is the less reliable, faster counterpart to TCP. It's characterized by being
connectionless, meaning it sends and receives messages without maintaining any state
between it and the receiver (sometimes also called "fire and forget") [[Spiceworks]](#bibliography). This can be partially
"side-stepped" by using connected UDP sockets (described more in the respective subsection).
The UDP is commonly used in applications that do not require strict reliability
when it comes to data transimission but are more performance-heavy (e.g. realtime applications, such as broadcasting) [[Spiceworks]](#bibliography).
The UDP is most recently specidfied in [[RFC768]](#bibliography).

#### Connected UDP sockets
Connected UDP sockets are UDP sockets that have a full 4 tuple (e.g. source and destination ip address and port) associated.
They are preferable for client applications (such as this project) and outbound traffic in general, due to optimizing route
lookup using a connection struct [[Cloudflare]](#bibliography). In addition to that, in the case of this project they made communication with the server
via UDP much more comfortable (described more in the appropriate section). They work by enabling an application to associate
the socket with the socket name of a peer [[IBM]](#bibliography). This enables the socket to use methods/functions like Receive() and Send() instead
of ReceiveFrom() or SendTo() since the destination/source is always known.

## Program usage 
To run the program, follow these steps:
1. Run `make` in the root folder of this repository
2. Run `./ipk25chat-client` [ARGS], where [ARGS] are:<br>
    **1. -t**: The protocol to use<br>
    **2. -s**: The IP address to use (or hostname to be translated into it)<br>
    **3. -p**: The port of the server. Default value is 4567<br>
    **4. -d**: The timeout to wait for one confirmation message in the UDP variant. Given in milliseconds. Default value is 250.<br>
    **5. -r**: Maximum number of UDP retransmissions until a CONFIRM message is received. Default value is 3.<br>
    **6. -h**: Help argument. Prints the guide top the program and exits.<br>
    **7. --discord**: Bonus argument, used for integration for the reference discord server. Enables the `discord.CHANNEL_ID` notation in addition to `CHANNEL_ID` notation.<br>


## Implementation
The following section describes some interesting parts of the application and how they were solved. The first part
describes shared behaviour between the two variants (e.g. user input processing, client FSM behaviour...). The second
and third part describe the parts which are unique respectively for the TCP and UDP variant.

### Shared behaviour between variants
The two variants share most of their behaviour. The main difference is how they communicate with the server. However
user input processing, FSM logic, classes for commands/messages are the same for both variants, and server input validation 
according to the current state is mutual for both variants. On a high level, the behaviour of the client is described by the following 
diagram:<br><br> ![Client overview](/UML/IPK-SUMMARY.png)<br><br>
*Note: In this class diagram (and the following  class diagrams), interfaces are displayed as green, abstract classes as pink, normal classes as blue and structs as red.*<br>
The two variants also share the same exit codes. These are:<br>

| Exit code | Description                                                       |
| --------- | ----------------------------------------------------------------- |
| 0         | Success, normal program run                                       |
| 10        | An ERR message was received from the server                       |
| 20        | A malformed message was received from the server                  |
| 30        | A UDP message did not receive a CONFIRM response in time          |
| 40        | A request message did not receive a reply in time                 |
| 50        | A message received from server is not valid in the current state  |
| 60        | All other errors (for example a connection refused)               |

#### Client class
We can observe that in the diagram, the Client class is the most "important", e.g. connects all the other components of the program
and handles it's logic on a high level. This class implements a finite state machine representing client behaviour. The class 
runs in a loop and reacts to events, until one of these events ends the program (a BYE message from the server, the user exits, or a invalid message from the server is received). These events are invoked by the classes that compose Client, such as **UserInputReader** or **IServerCommunicator**.
The client also delegates low-level tasks such as reading user/server input and sending messages to the server to these classes.
Each state is implemented as one method which reacts to user/server input in a specific way. Each incoming server/user input
is validated (the validness of each input in each state can be seen from the todo link section).

#### User IO
The process for reading user input is driven by the **UserInputReader**, **UserInputValidator**, and **InputQueue** classes.
These can be observed from the diagram above. Upon startup, **UserInputReader** runs a background task which reads user input
in a loop and raises the `UserInputReceived` event for each input. The client, which is subscribed to this event, passes this
input to the **UserInputQueue** class. When the client is ready to process the next user input, it requests it from the queue.
The queue is internally guarded by a semaphore, which is released once for each enqueued input, ensuring only valid access.
When the client is ready to process the next user input, it calls the `Dequeue()` method which waits on the semaphore until user input is avaliable.
After getting an input, **UserInputValidator** is run by the client to verify whether the user input is acceptable in the current state.
A variant of this process with a single mesage is visualized by the following sequence diagram:<br><br>![User input processing](/UML/Sequence.png)<br><br>
*Note: Since user input is to be buffered until the client can process it, the user pressing CTRL + C/D is equivalent to enqueuing null. Upon processing all previous user inputs and requesting the next one, null is dequeued, upon which the client invokes the OnEofReceived method.*<br>

#### Processing messages with the server
Although the TCPServerCommunicator and UDPServerCommunicator work differently "under the hood", since their interface is the same the client processes server messages in both variants in the same way. They are both implemented as loops which call the appropriate method for receiving, after that parsing the received content and invoking the **MessageReceived** event. When it comes to sending, both variants contain an async task for sending a message. When it comes to storing server messages, **InputQueue** type is used again, this time with a template parameter of **Message**. Queueing server messages is probably not neccessary, but i chose this option as opposed to just storing the latest message because if two users typed in something at the same time, the last message could be overwritten before the client could process it. This ensures it would get processed sooner or later. In each state, the client either runs a **Task.WhenAny** containing two tasks where one is completed when the next user input is dequeued, one when the next server message is dequeued and reacts appropriately to what he gets first, or a **Task.WhenAny** containing one task waiting for the next server message and the other being a timeout (e.g. cases where the client is waiting for a reply).

#### Input validation
The **UserInputValidator** class tries to convert a raw input string into a **IReadable**, that is it tries to parse the input
as a command or a message of type MSG. In addition to that it decides whether the input (if it's structure is decided to be valid) is valid
at the current client state. Similiar logic is utilized when processing server input, since each **IReadable** instance has a `IsValid(State clientState)`
method indicating if it is valid in the current state. 

#### Message and Command classes and their types
The following diagram shows more in depth how messages and commands are represented in the program:<br><br>![IReadable diagram](/UML/IReadable.png)<br><br>
Each message/command type has it's own subclass. In addition to the before mentioned validation method, each type has it's own parameters. Messages that can be sent from
the client also have impelementations of `ToString()` and `AsBytes()` methods, which convert the message to a format suitable for sending to the server in the given protocol.
Messages that can be received from the server also have either a regular expression (TCP) or a parsing method (UDP) which is used to identify incoming messages from the server.
The **MalformedMessage** type is used to represent messages that don't fit into any other type.

### The TCP variant
#### Class diagram for this variant
![TCPServerCommunicator](/UML/TCPCommunicator.png)
#### Description of the unique features of this variant
The only non-shared feature in the TCP variant is the **TCPServerCommunicator**. It works by running in a loop until it's cancellation token source is cancelled. It utilizes **StreamReader** and **StreamWriter** to comfortably read and write messages in a textual form from the server. The `GetMessage()` method is used to receive messages, which works by reading one char at a time from the server (into a buffer of size one) until the last two characters received are `\r\n` (CRLF). This approach may be slower than reading more at once, but seemed like the least error-prone and safest option. Upon reaching CRLF, the method returns the received string to the main loop which calls `Message.Parse()`, which tries to parse the message using regular expressions for each message type. If it fails to do so, it returns a **MalformedMessage** object, upon which the client reacts by terminating the connection.

### The UDP variant
#### Class diagram for this variant
![UDPServerCommunicator](/UML/UDPCommunicator.png)
#### Description of the unique features of this variant
In addition to the **UDPServerCommunicator** as the unique feature, the **UDPClient** class also overrides some methods from the superclass, concretely the parts where user input is processed and a message is sent, since the client has to get a confirmation. In case it doesn't, the **OnMessageTimeout** method is invoked, which ends the program with an error code. When it comes to the communication with the server, the **UDPServerCommunicator** utilizes a connected UDP socket. At first, when sending the initial AUTH message, the socket is unconnected. For this period, `SendToAsync()` and `ReceiveFromAsync()` are used for communication. Each incoming datagram's sender IP address is then inspected, and if it does not match the server's, the datagram is dropped. When the server sends a datagram to the client from the allocated port, the communicator calls `Connect()` and maintains this connection for the rest of the program run. This allows the communicator to call methods like `ReceiveAsync()` and `SendAsync()` and not worry about where the datagrams may come from or arrive at. An issue the communicator has to solve is message confirmation. For this, it keeps a dictionary of sent messages (which will probably always have a size of 1, since the communicator waits for confirmation before sending another message, except CONFIRM which the communicator sends without waiting). The key is their ID and the value is a **MessageStateInformation** structure. This structure contains a task completion source which is set to true when the message is confirmed, and it's task, which is awaited until the message is confirmed (or it times out). On confirm, key value pairs with the key being the confirmed message ID are removed from the dictionary, unless they are request messages (AUTH or JOIN) where they are removed after a confirm and a reply. Confirm or reply messages that have invalid ref message ID's are dropped. On a message confirmation timeout, a **ConfirmTimeouted** event is invoked, leading to the client terminating the program. Incoming datagrams are processed by calling `Message.Parse()` which calls the parsing method for each message type and returns the result (or a **MalformedMessage** object). Then the communicator either invokes the **MessageReceived** event, or just sends a confirm (in the case of a PING message) or sets a task waiting for confirmation to completed (in the case of a CONFIRM message).

## Testing
This section shows some scenarios which the program was tested on, manually and with input piped from a file (found in the TestInputs/). In general, testing for both variants can be split into 3 parts --> functionality (e.g. sending and receiving messages which are mostly identical with some variant-specific cases), invalid cases, where the client is supposed to either terminate with a error code or show a local error, and the third part shows some captured pcaps (found in the *Pcaps/* folder) which display communication on the reference server. Each of the variants has one pcap file displaying a short conversation (auth, message and bye) and a longer one with renaming and joining channels. For each variant a mock server was used for testing: found in the *Server/* folder. These servers share the same interface, as they provide some simple user commands to send replies to the client. These are:<br>
    1. **msg [CONTENT]**: Sends a MSG message with [CONTENT].
    2. **reply [CONTENT]**: Sends a positive REPLY message with [CONTENT].
    3. **reply! [CONTENT]**: Sends a negative REPLY message with [CONTENT].
    4. **bye**: Sends a BYE mesasge.
    5. **err [CONTENT]**: Sends a ERR message with the given content.
    6. **ping**: **UDPServer.py** only. Sends a PING message.
    7. **malformed [CONTENT]**: Sends just [CONTENT], encoded into a byte array or as a string terminated with `\r\n` based on the protocol.

### TCP testing
Startup for all tests in this section was done by running `python3 Server/TCP/TCPServer.py` and `./ipk25chat-client -t tcp -s localhost -p 4567` with either a input file piped into the client or a series of commands typed in (one of these is described in each case), and a series of commands typed into the server.

### Functionality
### Test case 1: Successful authentication
**Input file**: TestInputs/auth

**Server commands**:
```
reply ano
```

**Client output**:
```
Action Success: ano
```

**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k

TYPE: BYE
DISPLAYNAME: l
```

### Test case 2: Unsuccessful authentication
**Input file**: TestInputs/auth

**Server commands**:
```
reply! nie
```

**Client output**:
```
Action Failure: nie
```
**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k

TYPE: BYE
DISPLAYNAME: l
```

### Test case 3: Sending a message
**Input file**: TestInputs/one_message

**Server commands**:
```
reply ano
```

**Client output**:
```
Action Success: ano
```

**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k

TYPE: MSG
DISPLAYNAME: l
CONTENT: ahoj
TYPE: BYE
DISPLAYNAME: l
```

### Test case 4: Joining a channel and then sending a message
**Input file**: TestInputs/join

**Server commands**:
```
reply ano
reply ano
```

**Client output**:
```
Action Success: ano
Action Success: ano
```

**Server output**:
```
TYPE: AUTH
USERNAME: a
DISPLAYNAME: c
SECRET: b
TYPE: JOIN
DISPLAYNAME: c
TYPE: MSG
DISPLAYNAME: c
CONTENT: toto
TYPE: BYE
DISPLAYNAME: c
```

### Test case 5: Renaming and then sending a message
**Input file**: TestInputs/rename

**Server commands**:
```
reply ano
```

**Client output**:
```
Action Success: ano
```

**Server output**:
```
TYPE: AUTH
USERNAME: ahoj
DISPLAYNAME: ahoooj
SECRET: ahooj
TYPE: MSG
DISPLAYNAME: nove_meno
CONTENT: premenovana
TYPE: BYE
DISPLAYNAME: nove_meno
```

### Test case 6: Sending a message and receiving one from the server
**Client commands**:
```
/auth j k l
ahoj server!
// press CTRL + C
```

**Server commands**:
```
reply ano
msg ahoj klient!
```

**Client output**:
```
Action Success: ano
SERVER: ahoj klient!
```

**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: MSG
DISPLAYNAME: l
CONTENT: ahoj server!
TYPE: BYE
DISPLAYNAME: l
```

### Test case 7: Exchanging messages with the server, then switching channel, renaming and exchanging some more
**Client commands**:
```
/auth j k l
ahoj server!
vyborne!
idem do ineho kanalu
/join kanal
/rename klient
uz sa volam klient!
// Press CTRL + C
```

**Server commands**:
```
reply ok
msg ahoj klient!
msg ako sa mas?
reply ano
msg jupi maj sa!
```

**Client output**:
```
Action Success: ok
SERVER: ahoj klient!
SERVER: ako sa mas?
Action Success: ano
SERVER: jupi maj sa!
```

**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: MSG
DISPLAYNAME: l
CONTENT: ahoj server!
TYPE: MSG
DISPLAYNAME: l
CONTENT: vyborne!
TYPE: MSG
DISPLAYNAME: l
CONTENT: idem do ineho kanalu
TYPE: JOIN
DISPLAYNAME: l
TYPE: MSG
DISPLAYNAME: klient
CONTENT: uz sa volam klient!
TYPE: BYE
DISPLAYNAME: klient
```

### Testing invalid cases
### Test case 1: Receiving a malformed message from the server
**Input file**: TestInputs/auth

**Server commands**:
```
malformed zla sprava
```

**Client output**:
```
ERROR: Malformed message received
```

**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: ERR
DISPLAYNAME: l
CONTENT: Malformed message received
```

**Client exit code**: 20 (Malformed message received)

### Test case 2: Receiving a MSG message in auth state
**Input file**: TestInputs/auth

**Server commands**:
```
msg ok
```

**Client output**:
```
ERROR: Invalid message MSG for state AUTH
```

**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: ERR
DISPLAYNAME: l
CONTENT: Invalid message type MSG in state AUTH
```

**Client exit code**: 50 (invalid message)

### Test case 3: Receiving a REPLY message in open state
**Input file**: TestInputs/auth

**Client commands**:
```
/auth j k l
```

**Server commands**:
```
reply ok
reply ok
```

**Client output**:
```
Action Success: ano
ERROR: Invalid message REPLY for state OPEN
```

**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: ERR
DISPLAYNAME: l
CONTENT: Invalid message type REPLY in state OPEN
```

**Client exit code**: 50 (invalid message)

### Test case 4: Timeout when waiting for a reply
**Input file**: TestInputs/auth<br>
**Server commands**: None<br>
**Client output**:
```
ERROR: Timeout when waiting for reply to authentication
```

**Client exit code**: 40 (Reply timed out)<br>
**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: ERR
DISPLAYNAME: l
CONTENT: Timeout when waiting for reply to authentication
```

### Test case 5: Trying to send too long message
**Input file**: TestInputs/too_long_msg_tcp
**Client output**:
```
Action Success: j
ERROR: Message too long. Truncating to 60000
```

**Server output**:
```
TYPE: AUTH
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: MSG
DISPLAYNAME: l
CONTENT: b * 60000
TYPE: BYE
DISPLAYNAME: l
```
**Client exit code**: 0

### Captured pcap files from the reference server
### Capture 1: Short conversation
**Description**: This capture shows a client authenticating successfully, sending a message and logging out.<br>
**Equivalent pcap file**: Pcaps/TCP/short_server_convo.pcapng<br>
**Image**: ![TCP short convo](/Screenshots/tcp_short.png)

### Capture 2: Long conversation
**Description**: This capture shows a client first disconnecting while in start, then connecting and authenticating, then writing a message and receiving some messages from other users, then joining a channel, renaming and listening for a while, then rejoining the default channel and disconnecting.<br>
**Equivalent pcap file**: Pcaps/TCP/long_server_convo.pcapng<br>
**Image**: ![TCP long convo](/Screenshots/tcp_long.png)

### UDP testing
For this variant, it's run the same as the TCP one except we are using `UDP/UDPServer.py`. The cases are also mostly equivalent, except this cariant contains some extra cases which deal with confirmation timeouts and retransmissions.

### Functionality
### Test case 1: Successful authentication
**Input file**: TestInputs/auth<br>
**Server commands**:
```
reply ano
```
**Client output**:
```
Action Success: ano
```
**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: CONFIRM
REFID: 0
TYPE: BYE
ID: 1
DISPLAYNAME: l
```

### Test case 2: Unsuccessful authentication
**Input file**: TestInputs/auth<br>
**Server commands**:
```
reply! ne
```
**Client output**:
```
Action Failure: ne
```
**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: CONFIRM
REFID: 0
TYPE: BYE
ID: 1
DISPLAYNAME: l
```

### Test case 3: Sending a message
**Input file**: TestInputs/one_message

**Server commands**:
```
reply ok
```

**Client output**:
```
Action Success: ok
```

**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: CONFIRM
REFID: 0
TYPE: MSG
ID: 1
DISPLAYNAME: l
CONTENT: ahoj server!
TYPE: BYE
ID: 2
DISPLAYNAME: l
```

### Test case 4: Joining a channel and then sending a message
**Input file**: TestInputs/join<br>
**Server commands**:
```
reply ano
reply ano
```
**Client output**:
```
Action Success: ano
Action Success: ano
```
**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: a
DISPLAYNAME: c
SECRET: b
TYPE: CONFIRM
REFID: 0
TYPE: JOIN
ID: 1
DISPLAYNAME: c
TYPE: CONFIRM
REFID: 1
TYPE: MSG
ID: 2
DISPLAYNAME: c
CONTENT: toto je sprava do superkanalu!
TYPE: BYE
ID: 3
DISPLAYNAME: c
```

### Test case 5: Renaming and then sending a message
**Input file**: TestInputs/rename<br>
**Server commands**:
```
reply ok
```
**Client output**:
```
Action Success: ok
```
**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: ahoj
DISPLAYNAME: ahoooj
SECRET: ahooj
TYPE: CONFIRM
REFID: 0
TYPE: MSG
ID: 1
DISPLAYNAME: nove_meno
CONTENT: premenovana sprava
TYPE: BYE
ID: 2
DISPLAYNAME: nove_meno
```

### Test case 6: Sending a message and receiving one from the server
**Client commands**:
```
/auth j k l
ahoj server!
// press CTRL + C
```

**Server commands**:
```
reply ano
msg ahoj klient!
```

**Client output**:
```
Action Success: ano
SERVER: ahoj klient!
```

**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: CONFIRM
REFID: 0
TYPE: MSG
ID: 1
DISPLAYNAME: l
CONTENT: ahoj server!
TYPE: CONFIRM
REFID: 1
TYPE: BYE
ID: 2
DISPLAYNAME: l
```

### Test case 7: Exchanging messages with the server, then switching channel, renaming and exchanging some more
**Client commands**:
```
/auth j k l
ahoj server!
vyborne!
idem do ineho kanalu
/join kanal
/rename klient
uz sa volam klient!
// Press CTRL + C
```

**Server commands**:
```
reply ok
msg ahoj klient!
msg ako sa mas?
reply ano
msg jupi maj sa!
```

**Client output**:
```
Action Success: ok
SERVER: ahoj klient!
SERVER: ako sa mas?
Action Success: ano
SERVER: jupi maj sa!
```

**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
reply ok
TYPE: CONFIRM
REFID: 0
TYPE: MSG
ID: 1
DISPLAYNAME: l
CONTENT: ahoj server!
msg ahoj klient!
TYPE: CONFIRM
REFID: 1
msg ako sa mas?
TYPE: CONFIRM
REFID: 2
TYPE: MSG
ID: 2
DISPLAYNAME: l
CONTENT: vyborne!
TYPE: MSG
ID: 3
DISPLAYNAME: l
CONTENT: idem do ineho kanalu
TYPE: JOIN
ID: 4
DISPLAYNAME: l
reply ok
TYPE: CONFIRM
REFID: 3
TYPE: MSG
ID: 5
DISPLAYNAME: klient
CONTENT: uz sa volam klient!
msg jupi maj sa!
TYPE: CONFIRM
REFID: 4
TYPE: BYE
ID: 6
DISPLAYNAME: klient
```

### Testing invalid cases
### Test case 1: Receiving a malformed message from the server
**Input file**: TestInputs/auth

**Server commands**:
```
malformed zla sprava
```

**Client output**:
```
ERROR: Malformed message received
```

**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
malformed zla sprava
TYPE: CONFIRM
REFID: 24940
TYPE: ERR
ID: 1
DISPLAYNAME: l
CONTENT: Malformed message received
```

*Note: The client tried to pick up the message id from the given message and it sent a CONFIRM containing it, hence the weird value*


### Test case 2: Receiving a MSG message in auth state
**Input file**: TestInputs/auth

**Server commands**:
```
msg ok
```

**Client output**:
```
ERROR: Invalid message MSG for state AUTH
```

**Server output**:
```
TTYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: CONFIRM
REFID: 0
TYPE: ERR
ID: 1
DISPLAYNAME: l
CONTENT: Invalid message type MSG in state AUTH
```

**Client exit code**: 50 (invalid message)

### Test case 3: Receiving a REPLY message in open state
**Input file**: TestInputs/auth

**Client commands**:
```
/auth j k l
```

**Server commands**:
```
reply ok
reply ok
```

**Client output**:
```
Action Success: ok
ERROR: Malformed message received
```

**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
reply ok
TYPE: CONFIRM
REFID: 0
reply ok
TYPE: CONFIRM
REFID: 1
TYPE: CONFIRM
REFID: 1
TYPE: ERR
ID: 1
DISPLAYNAME: l
```

**Client exit code**: 20 (malformed message) - this code happens due to the UDP variant treating reply messages with invalid message ID's as malformed

### Test case 4: Timeout when waiting for a reply
**Input file**: TestInputs/auth<br>
**Server commands**: None<br>
**Client output**:
```
ERROR: Timeout when waiting for reply to authentication
```

**Client exit code**: 40 (Reply timed out)<br>
**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
```

### Test case 5: Trying to send too long message
**Input file**: TestInputs/too_long_msg_udp
**Client output**:
```
Action Success: j
ERROR: Message too long. Truncating to 1475
```

**Server output**:
```
TYPE: AUTH
ID: 0
USERNAME: j
DISPLAYNAME: l
SECRET: k
TYPE: CONFIRM
REFID: 0
TYPE: MSG
ID: 1
DISPLAYNAME: l
CONTENT: b * 1475
TYPE: BYE
ID: 2
DISPLAYNAME: l
```
**Client exit code**: 0

### Testing invalid cases

## Bibliography
[RFC9293] Eddy, W. *Transmission Control Protocol (TCP)* [online]. August 2022. [cited 2025-04-14]. DOI: 10.17487/RFC9293. Avaliable at: https://datatracker.ietf.org/doc/html/rfc9293#name-introduction<br>
[Techtarget] Yasar, K. *Transmission Control Protocol (TCP)* [online]. June 2024. [cited 2025-04-14]. Avaliable at: https://www.techtarget.com/searchnetworking/definition/TCP<br>
[Spiceworks] Basumallick, C. *TCP vs UDP: understanding 10 Key Differences* [online]. April 2022. [cited 2025-04-14]. Avaliable at: https://www.spiceworks.com/tech/networking/articles/tcp-vs-udp/<br>
[Cloudflare] Majkowski, M. *Everything you ever wanted to know about UDP sockets but were afraid to ask, part 1* [online]. November 2021. [cited 2025-04-14]. Avaliable at: https://blog.cloudflare.com/everything-you-ever-wanted-to-know-about-udp-sockets-but-were-afraid-to-ask-part-1/<br>
[IBM] *CONNECT* [online]. April 2023. [cited 2025-04-15]. Avaliable at: https://www.ibm.com/docs/en/zos/3.1.0?topic=functions-connect<br>
[RFC768] Postel, J. *User Datagram Protocol* [online]. [cited 2025-04-15]. DOI: 10.17487/RFC0768. Avaliable at: https://datatracker.ietf.org/doc/html/rfc768
