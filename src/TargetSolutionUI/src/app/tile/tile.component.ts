import { Component, OnInit, ViewChild, Output, Input, EventEmitter, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService } from 'ngx-bootstrap/modal';
import { BsModalRef } from 'ngx-bootstrap/modal/bs-modal-ref.service';
import { FormBuilder, FormGroup, Validators, FormControl, ValidatorFn, AbstractControl } from '@angular/forms';
//import { ExcelService } from '../shared/services/excel.service';
import { PagerService } from './pager.service';
import {
  trigger,
  state,
  style,
  animate,
  transition
} from '@angular/animations';

@Component({
  selector: 'app-tile',
  templateUrl: './tile.component.html',
  styleUrls: ['./tile.component.scss']
})
export class TileComponent implements OnInit {

  @Input() childgridData: any;
  @Output() EditClick: EventEmitter<String> = new EventEmitter<String>(); //creating an output event
  @Output() deleteClick: EventEmitter<String> = new EventEmitter<String>();
  @Output() createClick:EventEmitter<String> = new EventEmitter<String>();
  @Output() associateClick:EventEmitter<String> = new EventEmitter<String>();
  @Input() flagItemscheck: any;
  public modalRef: BsModalRef;
  options = [{ label: 'name' }, { label: 'address' }];
  public selectedAll:null;
public contentarray;
  public sortBy:null;
  public exportBy:null;
  public TruckList:any[];
  public DriverList:any[];
  config = {
    animated: false,
    keyboard: false,
    backdrop: true,
    ignoreBackdropClick: true,
    class: 'modal-sm'
  };
  configLargeModel={
    animated: false,
    keyboard: false,
    backdrop: true,
    ignoreBackdropClick: true,
    class:'modal-lg'
  }
  selected_games: any;
  rows = [];
  selected = [];
  temp = [];
  private allItems: any[];
  pager: any = {};
  // paged items
  pagedItems: any[];

  name: string;
  searchText: string = "";
  selected_count: number = 0;
  games: any[];
  flagItems:any[];


  constructor(private router: Router,
    private modalService: BsModalService,
 //   private excelService: ExcelService,
  private pagerService:PagerService) {

  }


  ngOnInit() {
    this.games = this.childgridData;

    this.setPage(1);
    this.flagItems=this.flagItemscheck;
    this.TruckList=[{"name":"truck One"},
    {"name":"truck Two"},
    {"name":"truck Three"},
    {"name":"truck four"},
    {"name":"truck Five"}];
    this.DriverList=[{"name":"driver One"},
    {"name":"driver Two"},
    {"name":"driver Three"},
    {"name":"driver four"},
    {"name":"driver Five"}]
  }
  setPage(page: number) {
    // get pager object from service
    this.pager = this.pagerService.getPager(this.games.length, page);

    // get current page of items
    this.pagedItems = this.games.slice(this.pager.startIndex, this.pager.endIndex + 1);
}

  exportToExcel() {
    //this.excelService.exportAsExcelFile(this.games, 'persons');
  }


  // Getting Selected Games and Count
  getSelected() {
    this.selected_games = this.games.filter(s => {
      return s.selected;
    });
    this.selected_count = this.selected_games.length;
  }
  selectAll() {
    if (this.selectedAll) {
      this.games = this.games.filter(g => {
        g.selected = true;
        return true;
      });
      this.getSelected();

    } else {
      this.searchText = "";
      this.games = this.games.filter(g => {
        g.selected = false;
        return true;
      });
      this.getSelected();

    }


  }


  //Clear term types by user
  clearFilter() {
    this.searchText = "";
  }


  confirm(g): void {

    this.deleteClick.emit(g);

    this.modalRef.hide();

  }

  decline(): void {
    this.modalRef.hide();
  }
  delete(g, template: TemplateRef<any>) {


    this.modalRef = this.modalService.show(template, this.config);

  }
  edit(event, author) {
    this.EditClick.emit(author);
  }
  associate(g,template : TemplateRef<any>)
  {
    this.modalRef = this.modalService.show(template, this.configLargeModel);

  }
  Associateconfirm(g): void {
    this.associateClick.emit(g);
    this.modalRef.hide();

  }
  createNewDetails() {
    this.createClick.emit();
  }

  onExportChange(event)
  {
if(event.target.value=="excel")
{
  this.exportToExcel();
}
  }


  /* 
    updateFilter(event) {
      const val = event.target.value.toLowerCase();
  
      const temp = this.temp.filter(function(d) {
        return d.name.toLowerCase().indexOf(val) !== -1 || !val;
      });
  
      this.rows = temp;
      this.table.offset = 0;
    } */

  //Delete Single Listed Game Tag
  /* deleteGame(id: number) {
    this.searchText = "";
    this.games = this.games.filter(g => {
      if (g.id == id)
        g.selected = false;
  
      return true;
    });
    this.getSelected();
  } */


}
export class Person {
  id: number;
  name: String;
  surname: String;
  age: number;
}
