import {EventEmitter, Injectable, Output} from '@angular/core';
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
  private _socket: WebSocket | undefined;

  get socket(): (WebSocket | undefined) {
    return this._socket;
  }

  private  _connection: boolean = true;

  get connection(): boolean {
    return this._connection;
  }

  @Output()
  connected = new EventEmitter<boolean>();

  /**
   * Start the websocket connection
   */
  connect(): void {
    this._socket = new WebSocket(environment.server);
    this.buffer = [];
    this._socket.addEventListener('message', this.onMessage);
    this._socket.addEventListener('open', this.onOpen);
    this._socket.addEventListener('close', this.onClose);
    this._socket.addEventListener('error', this.onError);
  }

  /**
   * Stop the websocket connection
   */
  disconnect(): void {
    if (!this._socket) {
      throw new Error('websocket not connected');
    }
    this._socket.removeEventListener('message', this.onMessage);
    this._socket.removeEventListener('open', this.onOpen);
    this._socket.removeEventListener('close', this.onClose);
    this._socket.removeEventListener('error', this.onError);
    this._socket.close();
    this._socket = undefined;
    this.buffer = undefined;
  }

  send(msg: OutgoingMessage): void {
    if (!this._socket) {
      throw new Error('websocket not connected');
    }
    if (this.buffer) {
      this.buffer.push(msg);
    } else {
      this._socket.send(JSON.stringify(msg));
    }
  }

  private onMessage = (event: MessageEvent): void => {
    const msg = JSON.parse(event.data);
    this.incoming.next(msg);
  };

  private onOpen = (event: Event): void => {
    console.log('websocket opened', event);
    this.connectedEmit(true);
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
    this.connectedEmit(false);
    console.info('websocket closed', event);
  };

  ngOnDestroy() {
    this.disconnect();
  }

  connectedEmit(status: boolean) {
    this.connected.emit(status);
  }
}
