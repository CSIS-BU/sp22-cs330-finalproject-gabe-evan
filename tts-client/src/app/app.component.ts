import {Component, OnInit} from '@angular/core';
import {TtsService} from "./tts.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  private board = new Array(3).fill(new Array(3).fill(-1));

  constructor(private ttsService: TtsService) {

  }

  placeTic(row: number, col: number) {
    if(this.board[row][col] < 0){
      this.ttsService.send({row, col});
    }
  }

  ngOnInit() {
    this.ttsService.connect();
  }
}
