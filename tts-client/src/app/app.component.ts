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
  messages = [];
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

    if(!msg.error){
      // ...
    }else{
      // ... show error to user ...
    }
  }

  ngOnInit() {
    this.ttsService.connect();

    this.ttsService.incoming.subscribe((msg: any) => this.handleMessage(msg));
  }
}
