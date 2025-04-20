# IPK, Project 2 - Client for a chat server, Makefile
# Author: Igor Lacko (xlackoi00)

# Final executable
EXECUTABLE=ipk25chat-client

.PHONY: all run build clean

# Flags for dotnet publish
FLAGS=-c Release --use-current-runtime -p:PublishSingleFile=true -p:AssemblyName=$(EXECUTABLE) -p:DebugType=None -p:DebugSymbols=False

all: build

build:
	dotnet publish $(FLAGS) -o .

run: 
	dotnet run

clean:
	rm -f $(EXECUTABLE) && cd IPK25-CHAT && rm -rf bin obj