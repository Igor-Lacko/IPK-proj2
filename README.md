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
of data (being able to solve problems such as packet loss), being connection-oriented, and providing a in-order byte stream. [1]. 
Because of these features, the TCP is commonly utilized in applications that require reliable data transfer 
(e.g. The World Wide Web, email, file transfer...)[2]. However, the reliablility guarantees can
introduce significant overhead, which is why the TCP isn't suitable for all use cases.
***
### User datagram protocol (UDP)
The UDP is the less reliable, faster counterpart to TCP. It's characterized by being
connectionless, meaning it sends and receives messages without maintaining any state
between it and the receiver (sometimes also called "fire and forget")[3]. This can be partially
"side-stepped" by using connected UDP sockets (described more in the respective subsection).
As mentioned, it is less reliable than the TCP and it utilizes best-effort delivery, meaning
it does not guarantee effective delivery of data or any quality of service[4]. The UDP
is commonly used in applications that do not require strict reliability
when it comes to data transimission but are more performance-heavy (e.g. realtime applications, such as broadcasting)[3].

#### Connected UDP sockets
Connected UDP sockets are UDP sockets that have a full 4 tuple (e.g. source and destination ip address and port) associated.
They are preferable for client applications (such as this project) and outbound traffic in general, due to optimizing route
lookup using a connection struct[5]. In addition to that, in the case of this project they made communication with the server
via UDP much more comfortable (described more in the appropriate section). They work by enabling an application to associate
the socket with the socket name of a peer[6]. This enables the socket to use methods/functions like Receive() and Send() instead
of ReceiveFrom() or SendTo() since the destination/source is always known.

## Program usage 
To run the program, follow these steps:
1. Run `make` in the root folder of this repository
2. Run `./ipk25chat-client` [ARGS], where [ARGS] are summarized by the following table[7]

| Argument shortcut | Argument name     | Default value, if any | Note, if any            |
| ----------------- | ----------------- | --------------------- | ----------------------- |
| -p                | Port number       | 4567                  | None                    |
| -s                | Hostname/address  | None, is mandatory    | Can't be a IPv6 address |
| -t                | Protocol          | None, is mandatory    | None                    |
| -d                | Timeout           | 250                   | In milliseconds         |
| -r                | Retransmissions   | 3                     | None                    |
| -h                | Help              | None, is an option    | Prints help and exits   |
| --discord         | Discord notation  | None, is an option    | Bonus argument. Enables supporting the extended notation (e.g. `discord.CHANNEL_ID`) for channel id's for compatibility with the reference server. | 

## Implementation
The following section describes some interesting parts of the application and how they were solved. The first part
describes shared behaviour between the two variants (e.g. user input processing, client FSM behaviour...). The second
and third part describe the parts which are unique respectively for the TCP and UDP variant.

### Shared behaviour between variants
The two variants share most of their behaviour. The main difference is how they communicate with the server. However
user input processing, FSM logic, classes for commands/messages are the same for both variants, and server input validation 
according to the current state is mutual for both variants. A high level behaviourof the client is described by the following 
diagram:<br><br> ![Client overview](/UML/IPK-SUMMARY.png)<br><br>
*Note: In the diagram (and the following diagrams), interfaces are displayed as green, abstract classes as pink, normal classes as blue and structs as red.*

#### Client class
We can observe that in the diagram, the Client class is the most "important", e.g. connects all the other components of the program
and handles it's logic on a high level. This class implements a finite state machine[7] representing client behaviour. The class 
runs in a loop and reacts to events, until one of these events ends the program (a BYE message from the server, the user exits, or a invalid message from the server is received). These events are invoked by the classes that compose Client, such as **UserInputReader** or **IServerCommunicator**.
The client also delegates low-level tasks such as reading user/server input and sending messages to the server to these classes.
Each state is implemented as one method which reacts to user/server input in a specific way. Each incoming server/user input
is validated (the validness of each input in each state can be seen from the todo link section).

#### User IO
The process for reading user input is driven by the **UserInputReader**, **UserInputValidator**, and **UserInputQueue** classes.
These can be observed from the diagram above. Upon startup, **UserInputReader** runs a background task which reads user input
in a loop and raises the *UserInputReceived* event for each input. The client, which is subscribed to this event, passes this
input to the **UserInputQueue** class. When the client is ready to process the next user input, it requests it from the queue.
The queue is internally guarded by a semaphore, which is released once for each enqueued input, ensuring only valid access.
When the client is ready to process the next user input, it calls the *Dequeue()* method which waits on the semaphore until user input is avaliable.
After getting an input, **UserInputValidator** is run by the client to verify whether the user input is acceptable in the current state.
A variant of this process with a single mesage is visualized by the following sequence diagram:<br><br>![User input processing](/UML/Sequence.png)<br><br>
*NOTE: Since user input is to be buffered until the client can process it, the user pressing CTRL + C/D is equivalent to enqueuing null. Upon dequeuing null, the client detects this and invokes the OnEofReceived method.*<br>
*NOTE: the `input` variable in the diagram is of type string?, e.g. still raw input, `validatedInput` is of type IReadable? (null if invalid), so parsed by the validator into a message/command*

#### Input validation
The **UserInputValidator** class tries to convert a raw input string into a **IReadable**, that is it tries to parse the input
as a command or a message of type MSG. In addition to that it decides whether the input (if it's structure is decided to be valid) is valid
at the current client state. Similiar logic is utilized when processing server input, since each **IReadable** instance has a method indicating
if it is valid in the current state. The following table summarizes at which state the user/server input is acceptable.<br>

| Input   | From    | START   | AUTH                                                          | OPEN    | JOIN    |
| ------- | ------- | ------- | ------------------------------------------------------------- | ------- | ------- |
| /auth   | User    | Valid   | Valid, if the previous attempt was unsuccessful, else invalid | Invalid | Invalid |
| /join   | User    | Invalid | Invalid                                                       | Valid   | Valid   |
| /rename | User    | Valid   | Valid                                                         | Valid   | Valid   |
| /help   | User    | Valid   | Valid                                                         | Valid   | Valid   |
| /status | User    | Valid   | Valid                                                         | Valid   | Valid   |
| MSG     | Both    | Invalid | Invalid                                                       | Valid   | Valid   |
| REPLY   | Server  | Invalid | Valid if waiting for a reply, else invalid                    | Invalid | Valid   |
| CONFIRM | Server  | Ignored | Valid                                                         | Valid   | Valid   |
| PING    | Server  | Valid   | Valid                                                         | Valid   | Valid   |

<br><br>
*NOTE: ERR and BYE are always marked as valid, e.g. processed normally*<br>
*NOTE: /join is valid in JOIN because it is enqueued and will be processed in OPEN anyway*

#### Message and Command classes and their types
The following diagram shows more in depth how messages and commands are represented in the program:<br><br>todo

## Bibliography
todo formatting for this and in text citations
https://datatracker.ietf.org/doc/html/rfc9293#name-introduction
https://cs.wikipedia.org/wiki/Transmission_Control_Protocol
https://www.spiceworks.com/tech/networking/articles/tcp-vs-udp/
https://en.wikipedia.org/wiki/Best-effort_delivery
https://blog.cloudflare.com/everything-you-ever-wanted-to-know-about-udp-sockets-but-were-afraid-to-ask-part-1/
https://www.ibm.com/docs/en/zos/2.4.0?topic=functions-connect
https://git.fit.vutbr.cz/NESFIT/IPK-Projects/src/branch/master/Project_2