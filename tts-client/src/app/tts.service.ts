import { Injectable } from '@angular/core';
import {Subject} from "rxjs";
import {environment} from "../environments/environment";

export interface IncomingMessage {
  // assume that we receive serialized json that adheres to this interface
}

export interface OutgoingMessage {
  // we send serialized json that adheres to this interface
}

@Injectable({
  providedIn: 'root'
})
export class TtsService {
  /**
   * Emit the deserialized incoming messages
   */
  readonly incoming = new Subject<IncomingMessage>();

  private buffer: OutgoingMessage[] | undefined;
  private socket: WebSocket | undefined;

  /**
   * Start the websocket connection
   */
  connect(): void {
    this.socket = new WebSocket(environment.server);
    this.buffer = [];
    this.socket.addEventListener('message', this.onMessage);
    this.socket.addEventListener('open', this.onOpen);
    this.socket.addEventListener('close', this.onClose);
    this.socket.addEventListener('error', this.onError);
  }

  /**
   * Stop the websocket connection
   */
  disconnect(): void {
    if (!this.socket) {
      throw new Error('websocket not connected');
    }
    this.socket.removeEventListener('message', this.onMessage);
    this.socket.removeEventListener('open', this.onOpen);
    this.socket.removeEventListener('close', this.onClose);
    this.socket.removeEventListener('error', this.onError);
    this.socket.close();
    this.socket = undefined;
    this.buffer = undefined;
  }

  send(msg: OutgoingMessage): void {
    if (!this.socket) {
      throw new Error('websocket not connected');
    }
    if (this.buffer) {
      this.buffer.push(msg);
    } else {
      this.socket.send(JSON.stringify(msg));
    }
  }

  private onMessage = (event: MessageEvent): void => {
    const msg = JSON.parse(event.data);
    this.incoming.next(msg);
  };

  private onOpen = (event: Event): void => {
    console.log('websocket opened', event);
    const buffered = this.buffer;
    if (!buffered) {
      return;
    }
    this.buffer = undefined;
    for (const msg of buffered) {
      this.send(msg);
    }
  };

  private onError = (event: Event): void => {
    console.error('websocket error', event);
  };

  private onClose = (event: CloseEvent): void => {
    console.info('websocket closed', event);
  };

  ngOnDestroy() {
    this.disconnect();
  }
}
