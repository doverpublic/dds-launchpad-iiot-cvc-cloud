import { Component, OnInit ,Output,EventEmitter,ViewChild,ElementRef,NgZone} from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { MouseEvent } from '@agm/core';
import { FormBuilder, FormGroup, Validators, FormControl, ValidatorFn, AbstractControl ,FormArray} from '@angular/forms';
import { CreateDispatchOfficeService } from "./createDispatchOffice.service";
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
  selector: 'app-create-and-update-dispatch-office',
  templateUrl: './create-and-update-dispatch-office.component.html',
  styleUrls: ['./create-and-update-dispatch-office.component.scss']
})
export class CreateAndUpdateDispatchOfficeComponent implements OnInit {
  dispatch: FormGroup;
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

  @Output()
  click = new EventEmitter();
  public searchControl: FormControl;


  constructor(private router: Router, private fb: FormBuilder, private data: CreateDispatchOfficeService,
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
    this.createDispatchForm();

  }
  
  groups = [

    {
      title: 'Dispatch Office details',
      content: 'Dispatch'
    },
    {
      title: 'Address info',
      content: 'Address'
    },
    {
      title: 'Contact details',
      content: 'Contact'
    },
    {
      title:"Preferences",
      content:'Preferences'
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
  createDispatchForm()
  {
    this.dispatch = this.fb.group({
      name: ['', Validators.compose([Validators.required, Validators.minLength(2)])],
      code: ['',Validators.compose([ Validators.required, Validators.minLength(2)])],
      description: [''],
      file:[''],

      address: ['',Validators.compose([ Validators.required, Validators.minLength(2)])],
      country: ['',Validators.required],
      state: ['',Validators.required],
      city: ['',Validators.required],
      zipcode: ['', Validators.compose([Validators.required, Validators.pattern('^[0-9]{5}(?:-[0-9]{4})?$')])],
      
      longitude: [''],
      latitude: [''],
      search: [''],
     
      itemRows: this.fb.array([    
       this.fb.group({
         'firstname': [''],
         'lastname': [''],
         'email': ['', [Validators.pattern(this.emailPattern)]],
         'phone':['', [Validators.pattern(this.mobnumPattern)]]
       })
      ]),
      ShiftItems:this.fb.array([  
        this.fb.group({
        'shiftname':[''],
      'shiftStartTime':[''],
      'shiftEndTime':[''],
      'timezone':['']
      })])
      
    });
    if (this.isEmpty(this.data.dispatchData)) {

      this.dispatch = this.fb.group({
        name: ['', Validators.compose([Validators.required, Validators.minLength(2)])],
        code: ['', Validators.compose([Validators.required, Validators.minLength(2)])],
        description: [''],
        file:[''],
        address: ['',Validators.compose([ Validators.required, Validators.minLength(2)])],
        country: ['',Validators.required],
        state: ['',Validators.required],
        city: ['',Validators.required],
        zipcode: ['', Validators.compose([Validators.required, Validators.pattern('^[0-9]{5}(?:-[0-9]{4})?$')])],
        longitude: [''],
        latitude: [''],
        search: [''],
      
        itemRows: this.fb.array([
         this.fb.group({
           'firstname': [''],
           'lastname': [''],
           'email': ['', [Validators.pattern(this.emailPattern)]],
           'phone':['', [Validators.pattern(this.mobnumPattern)]]
         })
        ]),
        ShiftItems:this.fb.array([  this.fb.group({
  
          'shiftname':[''],
        'shiftStartTime':[''],
        'shiftEndTime':[''],
        'timezone':['']
        })])
      });

    }else{
            
      this.dispatch = this.fb.group({
        name: [this.data.dispatchData.name, [Validators.required, Validators.minLength(2)]],
      code: [this.data.dispatchData.code, [Validators.required]],
      description: [this.data.dispatchData.description],
      file:[this.data.dispatchData.file],
      address: [this.data.dispatchData.address, Validators.required],
      country:  [this.data.dispatchData.country, Validators.required],
      state: [this.data.dispatchData.state, Validators.required],
      city: [this.data.dispatchData.city, Validators.required],
      zipcode: [this.data.dispatchData.zipcode, Validators.required],
      longitude: [this.data.dispatchData.longitude],
      latitude: [this.data.dispatchData.latitude],
      search: [''],
        itemRows: this.fb.array(this.patchForm(this.data.dispatchData.itemRows,"contact")),
        ShiftItems: this.fb.array(this.patchForm(this.data.dispatchData.ShiftItems,"shiftTime")),

      });
      this.indextest = _.findIndex(this.countries,this.data.dispatchData.country);
      this.indextestsate = _.findIndex(this.states,this.data.dispatchData.state);
      this.indextestcity = _.findIndex(this.cities,this.data.dispatchData.city);
    this.dispatch.controls['country'].setValue(this.countries[this.indextest], {onlySelf: true});
    this.onCountrySelect(this.data.dispatchData.country);
    this.onSateSelect(this.data.dispatchData.state);
     this.dispatch.controls['state'].setValue(this.states[this.indextestsate], {onlySelf: true});
     this.url = this.data.dispatchData.file;

  
  this.dispatch.controls['city'].setValue(this.cities[this.indextestcity], {onlySelf: true});  
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
        this.dispatch.controls['file'].setValue('', {onlySelf: true});
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
        this.dispatch.controls['file'].setValue('', {onlySelf: true});

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
         this.dispatch.patchValue({
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
  
  patchForm(data,patchtype) {
    if(patchtype=="contact")
    {
      return data.map(ctrl => {
        return this.fb.group({
          firstname: [ctrl.firstname],
          lastname: [ctrl.lastname],
          email: [ctrl.email],
          phone:[ctrl.phone]
        });
      });  
    }else
    {
      return data.map(ctrl => {
        return this.fb.group({
          
          'shiftname':[ctrl.shiftname],
        'shiftStartTime':[ctrl.shiftStartTime],
        'shiftEndTime':[ctrl.shiftEndTime],
        'timezone':[ctrl.timezone]
        });
      });  
    }
     
  }


  initItemRows() {
     
    return  this.fb.group({
        'firstname': [''],
        'lastname': [''],
        'email': [''],
        'phone':['']
     })
  }
  initShiftItemRows() {
     
  return  this.fb.group({
      'shiftname': [''],
      'shiftStartTime': [''],
      'shiftEndTime': [''],
      'timezone':['']
   })
  }

  addNewRow(value) {
    console.log(value);
    if(value=='contact')
    {
      const control = <FormArray>this.dispatch.controls['itemRows'];
      control.push(this.initItemRows());
      console.log(value,"contact");

    }else
    {
      const control = <FormArray>this.dispatch.controls['ShiftItems'];
      control.push(this.initShiftItemRows());
      console.log(value,"shift");

    }
      
  }

  deleteRow(index: number,value) {
    if(value=='contact')
    {
      const control = <FormArray>this.dispatch.controls['itemRows'];
      control.removeAt(index);
    }else
    {
      const control = <FormArray>this.dispatch.controls['ShiftItems'];
      control.removeAt(index);
    }
   
  }
  cancelEdit() {
    this.data.dispatchData = [];
    this.dispatch.reset();
    this.router.navigate(['/admin/dispatchoffice/viewDispatchOffice']);


  }
  isEmpty(obj) {
    for (var key in obj) {
      if (obj.hasOwnProperty(key))
        return false;
    }
    return true;
  }


  Submit({ value, valid }: { value: CreateDispatchOfficeService, valid: boolean }) {

    this.data.dispatchData=value;
    console.log(this.data.dispatchData);
    this.toastr.info('Successfully created');

    this.router.navigate(['admin/dispatchoffice']);
    this.dispatch.reset();


  }
  // google mapsinfo
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
    this.dispatch.controls['latitude'].setValue( $event.coords.lat, {onlySelf: true});
    this.dispatch.controls['longitude'].setValue( $event.coords.lng, {onlySelf: true});
  
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