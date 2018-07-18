import { Component, OnInit ,ViewChild} from '@angular/core';
import { Router } from '@angular/router';
import { CreateTrailerService } from './create-and-edit-trailer-data/createTrailer.service'
import {
  trigger,
  state,
  style,
  animate,
  transition
} from '@angular/animations';
import * as _ from "lodash";

@Component({
  selector: 'app-trailercompany',
  templateUrl: './trailercompany.component.html',
  styleUrls: ['./trailercompany.component.scss']
})
export class TrailercompanyComponent implements OnInit {

  ngOnInit() {


  }

}

