import {Component, OnInit} from '@angular/core';
import {TtsService} from "./tts.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  constructor(private ttsService: TtsService) {
  }


  ngOnInit() {
    this.ttsService.connect();

    this.ttsService.send("howdy howdy")
  }
}
