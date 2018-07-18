import { Component, OnInit ,ViewChild} from '@angular/core';
import { Router } from '@angular/router';
import { CreateDispatchOfficeService } from '../create-and-update-dispatch-office/createDispatchOffice.service';
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
  selector: 'app-view-dispatch-office',
  templateUrl: './view-dispatch-office.component.html',
  styleUrls: ['./view-dispatch-office.component.scss']
})
export class ViewDispatchOfficeComponent implements OnInit {
  gridDispatchData:any[];
  public createEditDispatch;
  functionalitychecklist=[{"name":"Dispatch Office","associate":true,"edit":true,"delete":true}];
  constructor(private dispatchserviceData:CreateDispatchOfficeService,private router:Router, private toastr: ToastrService) {
    this.gridDispatchData=[];
    /* [
      {
        id: "0",
        file: '',
        name: 'abc',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "",
        logitude: "",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
      id: "1",
        file: '',
        name: 'john',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
        id: "2",
        file: '',
        name: 'qtr',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
      id: "3",
        file: '',
        name: 'www',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
      id: "4",
        file: '',
        name: 'xyz',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
      id: "5",
        file: '',
        name: 'mno',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: true
      },
      {
      id: "6",
        file: '',
        name: 'str',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
      id: "7",
        file: '',
        name: 'drt',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
      id: "8",
        file: '',
        name: 'ter',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
      id: "9",
        file: '',
        name: 'trailer abc10',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      },
      {
      id: "10",
        file: '',
        name: 'trailer abc11',
        code: '101',
        description: "",
        address: "",
        country: {
          "id": "3",
          "sortname": "DZ",
          "name": "Algeria"
        },
        state: {
          "id": "119",
          "name": "Blidah",
          "country_id": "3"
        },
        city: {
          "id": "6148",
          "name": "Shiffa",
          "state_id": "119"
        },
        zipcode: "101",
        lattitude: "33",
        logitude: "44",
        itemRows: [
          {
            firstname: "barli",
            lastname: "john",
            email: "john@gmail.com",
            phone: "33333333",
            
          }
        ],
        location: "Algeria",
        contact: "Justin 1234567",
        trailers: 0,
        trucks: 2,
        users: 0,
        drivers: 0,
        selected: false
      }
    ]; */
 
   }

  ngOnInit() {
    if(!this.isEmpty(this.dispatchserviceData.dispatchData))
    {
      
      this.gridDispatchData=[];
       this.gridDispatchData.push(this.dispatchserviceData.dispatchData);

    }
  }
  createNewDispatchdeatil()
  {
    this.dispatchserviceData.dispatchData  = [];
    this.router.navigate(['admin/dispatchoffice/createUpdateDispatch']);
  
  }
  EditDispatch(event)
  {
    this.dispatchserviceData.dispatchData = event;
    this.router.navigate(['admin/dispatchoffice/createUpdateDispatch']);
  }
  deleteDispatch(event)
  {
    var index = _.findIndex(this.gridDispatchData, event)
    if (index !== -1) {

    this.gridDispatchData.splice(index,1);
      this.dispatchserviceData.dispatchData=[];
    }
  }
  createDispatchData()
  {
    this.dispatchserviceData.dispatchData  = [];

    this.router.navigate(['admin/dispatchoffice/createUpdateDispatch']);
  
  }
  AssociateWithDispatch(event)
  {
  console.log(event);
  }
  isEmpty(obj) {
    for (var key in obj) {
      if (obj.hasOwnProperty(key))
        return false;
    }
    return true;
  }

}

