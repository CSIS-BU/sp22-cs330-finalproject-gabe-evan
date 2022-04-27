# CS 330: Final Project
## Tic-tac-toe

[Presentation Video](https://drive.google.com/file/d/1-C0iTOGT0y2SMfbJ3VavWnMnXKFA6Se4/view?usp=sharing)

Human vs AI
- Session-based gameplay
- AI is made to stumble to improve gameplay

Project Stack
- Angular manages the web application
- .NET Core 3.1 manages the game state during the client connection

Backend
- .NET Server
- Kicks new connections to their own thread
- Using WebSocket protocol

Frontend
- Angular application
- Establishes connection to server
- Communicates via WebSocket

WebSocket Protocol Overview
- Handshake initiated by client as a standard HTTP request
- Handshake requests an upgrade to WebSocket
- Websocket allows for bi-directional communication
- Eliminates the need for Long Polling


Basic Usage (Player):

`$ docker-compose build`

`$ docker-compose up`

Go to [http://localhost:5006/](http://localhost:5006/)
