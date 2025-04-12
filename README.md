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
2. Run `./ipk25chat-client` [ARGS], where [ARGS] are summarized by the following table

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

#### User IO

## Bibliography
todo format
https://datatracker.ietf.org/doc/html/rfc9293#name-introduction
https://cs.wikipedia.org/wiki/Transmission_Control_Protocol
https://www.spiceworks.com/tech/networking/articles/tcp-vs-udp/
https://en.wikipedia.org/wiki/Best-effort_delivery
https://blog.cloudflare.com/everything-you-ever-wanted-to-know-about-udp-sockets-but-were-afraid-to-ask-part-1/
https://www.ibm.com/docs/en/zos/2.4.0?topic=functions-connect
