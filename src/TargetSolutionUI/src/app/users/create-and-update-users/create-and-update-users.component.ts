import { Component, OnInit ,Output,EventEmitter,ViewChild,ElementRef,NgZone} from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { MouseEvent } from '@agm/core';
import { FormBuilder, FormGroup, Validators, FormControl, ValidatorFn, AbstractControl ,FormArray} from '@angular/forms';
import { CreateUsersService } from "./createUsers.service";
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import {  } from 'googlemaps';
import { MapsAPILoader } from '@agm/core';
import * as _ from "lodash";
import { country } from '../../country';
import { state } from '../../state';
import { log } from 'util';
import { cities } from '../../cities';
import { Timezone } from '../../Timezone';

import { ToastrService } from 'ngx-toastr';
@Component({
  selector: 'app-create-and-update-users',
  templateUrl: './create-and-update-users.component.html',
  styleUrls: ['./create-and-update-users.component.scss']
})
export class CreateAndUpdateUsersComponent implements OnInit {
  users: FormGroup;
  public indextest;
  public isvalid;
  error: any = { isError: false, errorMessage: '' };
  customClass: string = 'customClass';
  isFirstOpen: false;
  oneAtATime: boolean = true;
  accordionName: any;
  obj: any;
  public indextestsate;
  public indextestcity;
  public countries;
  public states;
  public cities;
  public lng: number;
  public lat: number;
  pager: any = {};
public country:null;
public state:null;
public city:null;
  @ViewChild("search")
  public searchElementRef: ElementRef;
  // paged items
  pagedItems: any[];
  public TimeZoneData:any[];
  emailPattern = "^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$";
  mobnumPattern = "^((\\+91-?)|0)?[0-9]{10}$"; 
  usernamePattern = "^[a-z0-9_-]{8,15}$";

  constructor(private router: Router, private fb: FormBuilder, private data: CreateUsersService,
    private httpService: HttpClient,private mapsAPILoader: MapsAPILoader,
    private ngZone: NgZone,
    private activatedRoute: ActivatedRoute
    ,private toastr: ToastrService) {


      
      this.countries=country;
      this.states=state;
      this.cities=cities;
      this.TimeZoneData=Timezone;
     }

  ngOnInit() {
    // initial center position for the map
    this.lat = 12.9783;
    this.lng= 77.6647;
    this.createUsersForm();

  }
  groups = [

    {
      title: 'Users details',
      content: 'Users'
    },
    {
      title: 'Address info',
      content: 'Address'
    }
  ];
  add()
  {
  
  this.contactdeails.push({value:''});

  }
  remove(i)
  {
   this.contactdeails.splice(i, 1);

  } 
  statesList = [];
  citiesList = [];
  contactdeails=[{value:''}]
  selectedCountry=0;

  onCountrySelect(n) {
    this.statesList = [];
    this.citiesList = [];
      this.statesList = this.states.filter(function (s) {
  
      return s.country_id == n.id;
    });
   
  }
  onSateSelect(stateId) {
    this.citiesList = [];
    this.citiesList = this.cities.filter(function (i) {

      return i.state_id == stateId.id;
    });
  }
  onCitySelect(cityid) {
  }

  open(group, i) {
    if (this.accordionName == group) {

      this.groups[i]["isFirstOpen"] = false;
      this.accordionName = "";

    } else if (this.accordionName == undefined) {
      this.groups[i]["isFirstOpen"] = true;
      this.accordionName = this.groups[i];
    }
    else {
      group = this.groups[i];
      for (i in this.groups) {
        this.groups[i]["isFirstOpen"] = false;

      }

      this.oneAtATime = true;
      group.isFirstOpen = !group.isFirstOpen;
      this.accordionName = group;
    }



  }
  next(group, i) {
    group.isFirstOpen = !group.isFirstOpen;

    ++i;

    group = this.groups[i];
    group.isFirstOpen = !group.isFirstOpen;

  }
  redirect() {
    this.router.navigate(['admin']);

  }
  createUsersForm()
  {
    this.users = this.fb.group({
      
      RoleType:'System Admin',
      firstname: ['', Validators.compose([Validators.required, Validators.minLength(2)])],
      lastname: ['',Validators.compose([ Validators.required, Validators.minLength(2)])],
      ToggleActive:'Active',     
      file:[''],
      username:['',Validators.compose([Validators.required, Validators.minLength(8),Validators.maxLength(15),Validators.pattern(this.usernamePattern)])],
      email:['',Validators.compose([Validators.required,Validators.pattern(this.emailPattern)])],
      address: ['',Validators.compose([ Validators.required, Validators.minLength(2)])],
      country: ['',Validators.required],
      state: ['',Validators.required],
      city: ['',Validators.required],
      zipcode: ['', Validators.compose([Validators.required, Validators.pattern('^[0-9]{5}(?:-[0-9]{4})?$')])],
      phonenumber: [''],
      fax: ['']
      
    });
    if (this.isEmpty(this.data.UsersData)) {

      this.users = this.fb.group({
        RoleType:'System Admin',
        firstname: ['', Validators.compose([Validators.required, Validators.minLength(2)])],
        lastname: ['',Validators.compose([ Validators.required, Validators.minLength(2)])],
        ToggleActive:'Active',     
        file:[''],
        username:['',Validators.compose([ Validators.required, Validators.minLength(8),Validators.maxLength(15),Validators.pattern(this.usernamePattern)])],
        email:['',Validators.compose([Validators.required,Validators.pattern(this.emailPattern)])],
        address: ['',Validators.compose([ Validators.required, Validators.minLength(2)])],
        country: ['',Validators.required],
        state: ['',Validators.required],
        city: ['',Validators.required],
        zipcode: ['', Validators.compose([Validators.required, Validators.pattern('^[0-9]{5}(?:-[0-9]{4})?$')])],
        phonenumber: [''],
        fax: ['']
         });

    }else{
            console.log("Eited condition");
      this.users = this.fb.group({
        RoleType:this.data.UsersData.RoleType,
      firstname: [this.data.UsersData.firstname, Validators.compose([Validators.required, Validators.minLength(2)])],
      lastname: [this.data.UsersData.lastname,Validators.compose([ Validators.required, Validators.minLength(2)])],
      ToggleActive:this.data.UsersData.ToggleActive,     
      file:[this.data.UsersData.file],
      username:[this.data.UsersData.username,Validators.compose([ Validators.required, Validators.minLength(8),Validators.maxLength(15),Validators.pattern(this.usernamePattern)])],
      email:[this.data.UsersData.email,Validators.compose([Validators.required,Validators.pattern(this.emailPattern)])],
      address: [this.data.UsersData.address, Validators.required],
      country:  [this.data.UsersData.country, Validators.required],
      state: [this.data.UsersData.state, Validators.required],
      city: [this.data.UsersData.city, Validators.required],
      zipcode: [this.data.UsersData.zipcode, Validators.compose([Validators.required, Validators.pattern('^[0-9]{5}(?:-[0-9]{4})?$')])],
      phonenumber: [this.data.UsersData.phonenumber],
      fax: [this.data.UsersData.fax]
  
      
      });
      this.indextest = _.findIndex(this.countries,this.data.UsersData.country);
      this.indextestsate = _.findIndex(this.states,this.data.UsersData.state);
      this.indextestcity = _.findIndex(this.cities,this.data.UsersData.city);
    this.users.controls['country'].setValue(this.countries[this.indextest], {onlySelf: true});
    this.onCountrySelect(this.data.UsersData.country);
    this.onSateSelect(this.data.UsersData.state);
     this.users.controls['state'].setValue(this.states[this.indextestsate], {onlySelf: true});
    this.url = this.data.UsersData.file;
     this.users.controls['city'].setValue(this.cities[this.indextestcity], {onlySelf: true});  
    }


  }

  url = '';
  validateFile(name: String) {
    var ext = name.substring(name.lastIndexOf('.') + 1);
    if (ext.toLowerCase() == 'png') {
        return true;
    }else if (ext.toLowerCase() == 'jpg') {
      return true;
  }
    else {
        return false;
    }
  } 
  
  public imagefile:any[];
  onSelectFile(event) {
    this.url ="";
    if (event.target.files && event.target.files[0])
     {
      if (!this.validateFile(event.target.files[0].name)) {
        this.users.controls['file'].setValue('', {onlySelf: true});
        this.imagefile=[];
        this.error= {isError:true,errorMessage:"please upload jpg ,png file"}
        return {
          error: {isError:true,errorMessage:"please upload jpg ,png file"}
         };       

    }else
    {
      var maxSize=25600;
      if(event.target.files[0].size > 25600 )
      {
        this.users.controls['file'].setValue('', {onlySelf: true});

        this.error= {isError:true,errorMessage:"file size should be < 25kb"}
        return {
          error: {isError:true,errorMessage:"file size should be < 25kb"}
         };       

      }
      else
      {
        var file = event.dataTransfer ? event.dataTransfer.files[0] : event.target.files[0];
             var pattern = /image-*/;
             var reader = new FileReader();
        var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
     var i = Math.floor(Math.log(event.target.files[0].size) / Math.log(1024));
  
        this.imagefile=[{"name":event.target.files[0].name,
        "filesize":Math.round(event.target.files[0].size / Math.pow(1024, i))+"kb"}]
        reader.readAsDataURL(event.target.files[0]); // read file as data url
        reader.onload = (event:any) => { // called once readAsDataURL is completed
         this.url = event.target.result;
         this.users.patchValue({
          file: event.target.result
       });
        
      }
      this.error= {isError:false}
      return {
        error: {isError:false}
       };  
       
    }


     }
    }
  }

  cancelEdit() {
    this.data.UsersData = [];
    this.users.reset();
    this.router.navigate(['/admin/users/viewUsers']);


  }
  isEmpty(obj) {
    for (var key in obj) {
      if (obj.hasOwnProperty(key))
        return false;
    }
    return true;
  }


  Submit({ value, valid }: { value: CreateUsersService, valid: boolean }) {

    this.data.UsersData=value;
    console.log(this.data.UsersData);
    this.toastr.info('Successfully created');
    this.users.reset();

    this.router.navigate(['/admin/users/viewUsers']);

  }
  // google maps zoom level
  zoom: number = 15;

  

  clickedMarker(label: string, index: number) {
  }

  mapClicked($event: MouseEvent) {
    /* this.markers=[];
    this.markers.push({
      lat: $event.coords.lat,
      lng: $event.coords.lng,
      draggable: true
    });
   */
    this.users.controls['latitude'].setValue( $event.coords.lat, {onlySelf: true});
    this.users.controls['longitude'].setValue( $event.coords.lng, {onlySelf: true});
  
  }

  markerDragEnd(m: marker, $event: MouseEvent) {
  }

  markers: marker[] = [
    {
      lat: 12.9783,
      lng: 77.6647,
      label: 'A',
      draggable: true
    }
  ]
}

interface marker {
  lat: number;
  lng: number;
  label?: string;
  draggable: boolean;
}