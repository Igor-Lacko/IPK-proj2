# IPK, Project 2 - Client for a chat server, Makefile
# Author: Igor Lacko (xlackoi00)

# Final executable
EXECUTABLE=ipk25chat-client

# .csproj file
PROJECT=IPK25-CHAT/IPK_25_CHAT.csproj

.PHONY: all run build clean

# Flags for dotnet publish
FLAGS=-c Release --use-current-runtime -p:PublishSingleFile=true -p:AssemblyName=$(EXECUTABLE) -p:DebugType=None -p:DebugSymbols=False

all: build

build:
	dotnet publish $(PROJECT) $(FLAGS) -o .

run: 
	dotnet run --project $(PROJECT)

clean:
	rm -f $(EXECUTABLE) && cd IPK25-CHAT && rm -rf bin obj