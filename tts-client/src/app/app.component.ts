import {Component, OnInit} from '@angular/core';
import {TtsService} from "./tts.service";
import {Message} from "./message";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  board = new Array(3).fill(new Array(3).fill(-1));
  gameFinished = false;

  wins = 0;
  losses = 0;
  ties = 0;

  message = "";

  connected = true;

  constructor(private ttsService: TtsService) {
  }

  placeTic(row: number, col: number) {
    if(this.board[row][col] < 0){
      this.ttsService.send({row, col});
    }
  }

  handleMessage(msg:Message){
    console.log(msg);

    this.board = msg.board;

    this.wins = msg.wins;
    this.ties = msg.ties;
    this.losses = msg.losses;

    this.message = msg.message;

    if(!msg.error){
      if(msg.finished){
        this.askToReplay();
      }
    }else{
      if(msg.finished){
        this.askToReplay();
      }
    }
  }

  askToReplay(){
    this.gameFinished = true;
  }

  tellReplay(){
    this.ttsService.send({row: -1, col: -1});
    this.gameFinished = false;
  }

  ngOnInit() {
    this.ttsService.connect();
    this.ttsService.incoming.subscribe((msg: any) => this.handleMessage(msg));
    this.ttsService.connected.subscribe((status: boolean) => {
      this.connected = status;
    });
  }
}
