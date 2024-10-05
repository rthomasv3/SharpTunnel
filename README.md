# SharpTunnel
SharpTunnel is a work-in-progress attempt to create a free and open-source tunnel using C#. The main purpose is to expand my knowledge of servers and HTTP, but hopefully also become a useful tool in the end for self-hosting services without port forwarding or exposing your home IP.

The core idea is to have a web server that will receive requests, forward them over a web socket to any connected tunnels, which will then forward the request to the end server and return the response (like a self-hostable Cloudflare Tunnel).

The whole system is two parts - a public web server and tunnel service running inside your network.

## Tunnel
The tunnel will be configured with the address of the public web server. When it starts up, it will attempt to make a web socket connection to the web server. This is using SignalR with MessagePack serialization right now. It then listens for requests and will forward them to the local server based on the configuration. The local server response is then sent back up to the web server.

## Web Server
The web server will have a few responsibilities:
* Forwarding requests to the tunnel
* Managing web socket connections
* Serving any UI and API endpoints for configuration
* Caching responses
* Rate limiting
* Auth

## What's Working
The system currently works with simple HTTP requests. It's able to retrieve and display UI from the end server, login, and even stream some content.

## To Do
- [x] Proof-of-concept tunnel connection and request/response serialization
- [x] Basic HTTP requests with forms and bodies
- [ ] Files
- [ ] Web socket connections
- [ ] UI to configure forwarding settings (including settings storage, likely via SQLite database)
- [ ] Additional security and authentication
