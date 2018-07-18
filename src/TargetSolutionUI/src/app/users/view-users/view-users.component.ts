import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CreateUsersService } from '../create-and-update-users/createUsers.service';
import { ToastrService } from 'ngx-toastr';

import {
  trigger,
  state,
  style,
  animate,
  transition
} from '@angular/animations';
import  * as _  from  "lodash";

@Component({
  selector: 'app-view-users',
  templateUrl: './view-users.component.html',
  styleUrls: ['./view-users.component.scss']
})
export class ViewUsersComponent implements OnInit {

  gridUsersData: any[];
  public createEditUusers;
  functionalitychecklist = [{ "name": "Users", "associate": false, "edit": true, "delete": true }];


  constructor(private usersServiceData: CreateUsersService, private router: Router, private toastr: ToastrService) {
    this.gridUsersData = [];
   /*  [
      {
        id: "0",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'System Admin',
        ToggleActive:'Active',
        username: 'John',
        email:'johnAd1234@gmail.com',
        address: "First phase bangalore",
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
        selected: false
      },
      {
        id: "1",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'System Admin',
        ToggleActive:'InActive',
        username: 'John',
        email:'johnAd1234@gmail.com',
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
        selected: false
      },
      {
        id: "2",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'Terminal Manager',
        ToggleActive:'InActive',
        username: 'John',
        email:'johnAd1234@gmail.com',
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
        selected: false
      },
      {
        id: "3",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'Dispatcher',
        ToggleActive:'InActive',
        username: 'John',
        email:'johnAd1234@gmail.com',
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
        selected: false
      },
      {
        id: "4",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'System Admin',
        ToggleActive:'InActive',
        username: 'John',
        email:'johnAd1234@gmail.com',
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
        selected: false
      },
      {
        id: "5",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'Maintenance Technician',
        ToggleActive:'InActive',
        username: 'John',
        email:'johnAd1234@gmail.com',
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
        selected: false
      },
      {
        id: "6",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'System Admin',
        ToggleActive:'InActive',
        username: 'John',
        email:'johnAd1234@gmail.com',
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
        selected: false
      },
      {
        id: "7",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'Dispatcher',
        ToggleActive:'InActive',
        username: 'John',
        email:'johnAd1234@gmail.com',
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
        selected: false
      },
      {
        id: "8",
        file: '',
        firstname: 'Harry',
        lastname: 'Potter',
        RoleType:'Terminal Manager',
        ToggleActive:'InActive',
        username: 'John',
        email:'johnAd1234@gmail.com',
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
        selected: false
      }
    ];
 */
  }

  ngOnInit() {

    if (!this.isEmpty(this.usersServiceData.UsersData)) {

      this.gridUsersData = [];
      this.gridUsersData.push(this.usersServiceData.UsersData);

    }

  }


  createNewUsersdeatil() {
    this.usersServiceData.UsersData = [];

    this.router.navigate(['admin/users/createUpdateUsers']);

  }
  EditUser(event) {
    this.usersServiceData.UsersData = event;
    this.router.navigate(['admin/users/createUpdateUsers']);
  }
  deleteUser(event) {
    var index = _.findIndex(this.gridUsersData, event)
    if (index !== -1) {

      this.gridUsersData.splice(index, 1);
      this.usersServiceData.UsersData = [];
      this.toastr.info('User Deleted');

    }
  }
  createUsersData() {
    this.usersServiceData.UsersData = [];

    this.router.navigate(['admin/users/createUpdateUsers']);

  }
  isEmpty(obj) {
    for (var key in obj) {
      if (obj.hasOwnProperty(key))
        return false;
    }
    return true;
  }
}
