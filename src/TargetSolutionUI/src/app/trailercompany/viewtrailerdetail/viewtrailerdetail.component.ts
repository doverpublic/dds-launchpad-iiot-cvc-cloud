import { Component, OnInit ,ViewChild} from '@angular/core';
import { Router } from '@angular/router';
import { CreateTrailerService } from '../create-and-edit-trailer-data/createTrailer.service';
import { ToastrService } from 'ngx-toastr';

import {
  trigger,
  state,
  style,
  animate,
  transition
} from '@angular/animations';
import * as _ from "lodash";

@Component({
  selector: 'app-viewtrailerdetail',
  templateUrl: './viewtrailerdetail.component.html',
  styleUrls: ['./viewtrailerdetail.component.scss']
})
export class ViewtrailerdetailComponent implements OnInit {

  gridData:any[];
  parentMessage = "message from parent";
  public EditTrailer;
  message:string;
  functionalitychecklist=[{"name":"Trailer Company","associate":false,"edit":true,"delete":true}];



constructor(private router:Router,private createTrailerData:CreateTrailerService,
  private toastr: ToastrService) {
 
    this.gridData=[];
    /* [
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
        name: 'abc',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        
      itemRows: [
        {
          firstname:"barli",
          lastname:"john",
          email:"john@gmail.com",
          phone:"33333333",
        },
        {
          firstname: "test",
          lastname: "monu",
          email: "john@gmail.com",
          phone: "33333333"
        }
      ]
        ,
        location:"Algeria",
  
      contact:"Justin 1234567",
      trailers:0,trucks:2,users:0,drivers:0,
    
        id: 1,
        selected: false
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
  
        name: 'john',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
            
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
    
        id: 2,
        selected: false
      },
      {
  
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
  
        name: 'qtr',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
    
        id: 3,
        selected: false
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
  
        name: 'www',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
    
        id: 4,
        selected: false
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
  
        name: 'xyz',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
    
        id: 5,
        selected: false
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
  
        name: 'mno',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
    
        id: 6,
        selected: true
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
  
        name: 'str',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
    
        id: 7,
        selected: false
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
  
        name: 'drt',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
    
        id: 8,
        selected: false
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ78iCl73Mw5MeZCfgQdwnknfwLBNBYfFigujBw7oJHhY8qIl/gRNEFMFDRndFQQ8JePGY4SF4EXwDsgpefLPggkeDICgiG7umq2eqe7rn4+1kN2GnoDbJTHdNT9ev66v7bYcF8YbtN3U2Vy6cvXOnw+5n2mzy6u98cnPFPzzOUz63M7j2AGuppQrUAqWlSnROtzrtlLRkdNFajLKXQGksRgniHv93wHmiNQM/HbKRG5YKDOIByhiQqzHcSXjkrkv6O/zfEY7BoXdwDMtKLxbEE5Tj4pU1vocyBhqjkNhEp7gFigSKmFi/pPmUT/KsQDlzVI6NADB9LiO29AegrjSA1B0DyFgQgJhoKAHXFFDe4fxMA7q8qU0grA6Tdv/m/CLnf7XrH3N+zCK7f8dACWIPFcSSVcvYOFWmuAcA6uH9rlHRQQzvNzcqU8iYoBLAKvQtSj5FBUc4hkizEBLIfaN1C2Loe4JAi3AcoWEMQEcgv2rWc65kHp+wIKwu/aT9fprzRUvblzh/QX7Dy7++ZWvnEfM8VNwDTHQQD/m3U9I2KJCxzK14ISNCGV7iXnQXJCyBtAIqSISMGW/DECweAlon6a6k5VLfA/pkYPSTNmcMZneB3tCA8tpdcIm91JqYYgiwIEFcJsPRPnUZIPegQv8iGTOMM4piGxFT2WOhIAWb6LNuGijvcj6u0f6CZeXlXp/zrWQlMnae81Po7o5xfK+Stm9x/pG4iWbI5gpUk15GcWpZgniByopqjIIiccFlzApjEbtFYziXNgo1ax01DZSfLebORhcrtvseXdNnnK/itbcRKM9zfhiv/c75Q87/bdW2CBfgaQqo4n5n2M5Nsx5hhZaojOKMR1itKa52J/kMYp9kK3FJxkPB/EcFC8hKguadLbh9xPk2fn+W8+OcL5P7n28VJKAUEdifoLJ8Eg9EFaxSnASIEECqNEA5oLwVApEVuJahtiAd7L9IxieC2qbIrdpwl2KUXzl/w/kFjElgcp6TGTDn61sEyUKre0w15bFKe2LCYog4QrirASpjRKwTKPzIal2E1ViSTMrR6iF+Yi2K3NLIrR7TXdnsHVCA3kegAF0i179FIG0DJLQ4Bu5h3FDcE5PAc0zSWxcBs6wgI0jdmgDeiri24lpKvfhor1wP0HcYs+j06Raf6RotST5uKQKbg25hg/UKm1uxxwbS9YF7sgMvKIgtQktgm6/3ZGN19hUoQB9ov8FEf32Xnm2buEkFlxORmKQsczK5nYhkTW4JqE0pfETAMjKCQFybEOu53megACh+I7+/5PzPFp9HV+JcUZJYfSea8m1gWhIl+YqihHtbpGmwOXsJSYq8wD7UYvkk1rGVHaYEUGrgnFWfXZJJVU65y0r4oKSX8fsrnL+qmR7fwO+Q/h6SezdI+nzI8pXbNzm/h9+fJHIYCW7TMTdQwvdLrMaQqRt9oOyuYcWuSlJpUFDfGj+oWwlW51EYR5XLkNXnZIFYSvj6vld8ryzKIYKyYwCJjEn+4vyDASTN08idJgGnWvRa4wo9QAswY2VVTFG4mxrSaSmrWxhkCuUdWVb7EhU8LhlDmDwnH2/RMYSlYNSC7XtlUaoQFNd+YdlG21YsSkv7V0cxpcp/tiraP6DA/sv5Gu0f0SyVLQi8bQHErVY9+wmUy0wtqdehBxmUsM0EGc6jrSpa17PTRCL8ozTQlCV9WgrPyvy0ncwwplqpv08ODEn5+mEmB1Nmj9RRhuTA1CnL78V009RXfaZ8Rh/PvphO2o2TSq8YH5yZGZJMLYQDSnwuTPX85PBSGVCOMfu4U4ITaw9Z7u1KHCI2/4QCPUOhysOsyLRJOOFtbAUs2o9mMwt0xwdJP6GwOVMPE+VPsmV1HirPM9ZU7PtBgwTkmmwOiA4uHhjLjP8Oq1qUa8hN0NUdNiwRy/ZPpGJ7hsJTwPL7LDGmnj6m2CaAjZko5jkICrnfk20Olp2JUUmMLyvq9bTUvoyWCMpu1Q7t3/VkQGFEgUuDVQiQe4a9nzETFVkvt3JBttjci4g7cBEcUdIn23vRTf+K3FsZah2yCGiq1PoFcqnLamOUmhSSie8bFE73SVT3IxQeGGo+HgJrQyyBLH7JXd4Q61kAvhOtf7+gOCatoLQQgxquR5bw51UtUWtRVHM8QsCsifsYIBg6GNxOmXnjb6qUAIRbkMFvh4mK6wDdT4QK8hULUIdEtbiXcNW/9VH7B/iu7lmAstkTbp6E8hxDoKq7ohCthKv1lyVyaoXiNEMSn2sCsmHyvMw1qADNu56NYVc5YvbTd77W1/T3RpXP3nTu9//NoKU2mG2pQfpfgAEAjgwDHPWRp0oAAAAASUVORK5CYII=',
  
        name: 'ter',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
        id: 9,
        selected: false
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ78iCl73Mw5MeZCfgQdwnknfwLBNBYfFigujBw7oJHhY8qIl/gRNEFMFDRndFQQ8JePGY4SF4EXwDsgpefLPggkeDICgiG7umq2eqe7rn4+1kN2GnoDbJTHdNT9ev66v7bYcF8YbtN3U2Vy6cvXOnw+5n2mzy6u98cnPFPzzOUz63M7j2AGuppQrUAqWlSnROtzrtlLRkdNFajLKXQGksRgniHv93wHmiNQM/HbKRG5YKDOIByhiQqzHcSXjkrkv6O/zfEY7BoXdwDMtKLxbEE5Tj4pU1vocyBhqjkNhEp7gFigSKmFi/pPmUT/KsQDlzVI6NADB9LiO29AegrjSA1B0DyFgQgJhoKAHXFFDe4fxMA7q8qU0grA6Tdv/m/CLnf7XrH3N+zCK7f8dACWIPFcSSVcvYOFWmuAcA6uH9rlHRQQzvNzcqU8iYoBLAKvQtSj5FBUc4hkizEBLIfaN1C2Loe4JAi3AcoWEMQEcgv2rWc65kHp+wIKwu/aT9fprzRUvblzh/QX7Dy7++ZWvnEfM8VNwDTHQQD/m3U9I2KJCxzK14ISNCGV7iXnQXJCyBtAIqSISMGW/DECweAlon6a6k5VLfA/pkYPSTNmcMZneB3tCA8tpdcIm91JqYYgiwIEFcJsPRPnUZIPegQv8iGTOMM4piGxFT2WOhIAWb6LNuGijvcj6u0f6CZeXlXp/zrWQlMnae81Po7o5xfK+Stm9x/pG4iWbI5gpUk15GcWpZgniByopqjIIiccFlzApjEbtFYziXNgo1ax01DZSfLebORhcrtvseXdNnnK/itbcRKM9zfhiv/c75Q87/bdW2CBfgaQqo4n5n2M5Nsx5hhZaojOKMR1itKa52J/kMYp9kK3FJxkPB/EcFC8hKguadLbh9xPk2fn+W8+OcL5P7n28VJKAUEdifoLJ8Eg9EFaxSnASIEECqNEA5oLwVApEVuJahtiAd7L9IxieC2qbIrdpwl2KUXzl/w/kFjElgcp6TGTDn61sEyUKre0w15bFKe2LCYog4QrirASpjRKwTKPzIal2E1ViSTMrR6iF+Yi2K3NLIrR7TXdnsHVCA3kegAF0i179FIG0DJLQ4Bu5h3FDcE5PAc0zSWxcBs6wgI0jdmgDeiri24lpKvfhor1wP0HcYs+j06Raf6RotST5uKQKbg25hg/UKm1uxxwbS9YF7sgMvKIgtQktgm6/3ZGN19hUoQB9ov8FEf32Xnm2buEkFlxORmKQsczK5nYhkTW4JqE0pfETAMjKCQFybEOu53megACh+I7+/5PzPFp9HV+JcUZJYfSea8m1gWhIl+YqihHtbpGmwOXsJSYq8wD7UYvkk1rGVHaYEUGrgnFWfXZJJVU65y0r4oKSX8fsrnL+qmR7fwO+Q/h6SezdI+nzI8pXbNzm/h9+fJHIYCW7TMTdQwvdLrMaQqRt9oOyuYcWuSlJpUFDfGj+oWwlW51EYR5XLkNXnZIFYSvj6vld8ryzKIYKyYwCJjEn+4vyDASTN08idJgGnWvRa4wo9QAswY2VVTFG4mxrSaSmrWxhkCuUdWVb7EhU8LhlDmDwnH2/RMYSlYNSC7XtlUaoQFNd+YdlG21YsSkv7V0cxpcp/tiraP6DA/sv5Gu0f0SyVLQi8bQHErVY9+wmUy0wtqdehBxmUsM0EGc6jrSpa17PTRCL8ozTQlCV9WgrPyvy0ncwwplqpv08ODEn5+mEmB1Nmj9RRhuTA1CnL78V009RXfaZ8Rh/PvphO2o2TSq8YH5yZGZJMLYQDSnwuTPX85PBSGVCOMfu4U4ITaw9Z7u1KHCI2/4QCPUOhysOsyLRJOOFtbAUs2o9mMwt0xwdJP6GwOVMPE+VPsmV1HirPM9ZU7PtBgwTkmmwOiA4uHhjLjP8Oq1qUa8hN0NUdNiwRy/ZPpGJ7hsJTwPL7LDGmnj6m2CaAjZko5jkICrnfk20Olp2JUUmMLyvq9bTUvoyWCMpu1Q7t3/VkQGFEgUuDVQiQe4a9nzETFVkvt3JBttjci4g7cBEcUdIn23vRTf+K3FsZah2yCGiq1PoFcqnLamOUmhSSie8bFE73SVT3IxQeGGo+HgJrQyyBLH7JXd4Q61kAvhOtf7+gOCatoLQQgxquR5bw51UtUWtRVHM8QsCsifsYIBg6GNxOmXnjb6qUAIRbkMFvh4mK6wDdT4QK8hULUIdEtbiXcNW/9VH7B/iu7lmAstkTbp6E8hxDoKq7ohCthKv1lyVyaoXiNEMSn2sCsmHyvMw1qADNu56NYVc5YvbTd77W1/T3RpXP3nTu9//NoKU2mG2pQfpfgAEAjgwDHPWRp0oAAAAASUVORK5CYII=',
  
        name: 'trailer abc10',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
    
        id: 10,
        selected: false
      },
      {
        file:'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIoAAAAgCAYAAADNAODsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyFpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTQyIDc5LjE2MDkyNCwgMjAxNy8wNy8xMy0wMTowNjozOSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyN0NDNDAwMTVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyN0NDNDAwMjVGRjQxMUU4QkExM0Q4MzQ3NkZEMzI1RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI3Q0MzRkZGNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI3Q0M0MDAwNUZGNDExRThCQTEzRDgzNDc2RkQzMjVFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+WAd4QwAABtlJREFUeNrsXM2rI0UQ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',
  
        name: 'trailer abc11',
        code:'101',
        description:"",
        address:"",
        country:{
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state:
        {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city:{
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode:"101",
        lattitude:"33",
        logitude:"44",
        itemRows: [
          {
            firstname:"barli",
            lastname:"john",
            email:"john@gmail.com",
            phone:"33333333",
          }
        ],
        location:"Algeria",
        contact:"Justin 1234567",
        trailers:0,trucks:2,users:0,drivers:0,
        id: 11,
        selected: false
      }
    ]  ;*/  
}
createnewTrailerdeatil()
{

  this.router.navigate(['admin/trailercompany/Edit']);

}

EditGridParent(event)
{
  this.createTrailerData.trailerEdit = event;
  this.router.navigate(['admin/trailercompany/Edit']);

}
deleteGridParent(event)
{
  var index = _.findIndex(this.gridData, event)
  if (index !== -1) {

     this.gridData.splice(index,1);
     this.createTrailerData.trailerEdit = [];
  }
}

createGridData()
{
  this.createTrailerData.trailerEdit = [];

    this.router.navigate(['admin/trailercompany/Edit']);
 
}

  ngOnInit() {
    this.createTrailerData.currentMessage.subscribe(message => this.message = message)
   
    if(!this.isEmpty(this.createTrailerData.trailerEdit))
    {
      this.gridData=[];
      console.log("data is there");
       this.gridData.push(this.createTrailerData.trailerEdit);

    }

  }
  isEmpty(obj) {
    for (var key in obj) {
      if (obj.hasOwnProperty(key))
        return false;
    }
    return true;
  }
}
