import { Component, OnInit ,Output,EventEmitter,ViewChild,ElementRef,NgZone} from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { MouseEvent } from '@agm/core';
import { FormBuilder, FormGroup, Validators, FormControl, ValidatorFn, AbstractControl ,FormArray} from '@angular/forms';
import { CreateTrailerService } from "./createTrailer.service";
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import {  } from 'googlemaps';
import { MapsAPILoader } from '@agm/core';
import * as _ from "lodash";
import { country } from '../../country';
import { state } from '../../state';
import { log } from 'util';
import { cities } from '../../cities';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-create-and-edit-trailer-data',
  templateUrl: './create-and-edit-trailer-data.component.html',
  styleUrls: ['./create-and-edit-trailer-data.component.scss']
})
export class CreateAndEditTrailerDataComponent implements OnInit {
  public indextest;
  user: FormGroup;
  public isvalid;
  error: any = { isError: false, errorMessage: '' };
  customClass: string = 'customClass';
  isFirstOpen: false;
  oneAtATime: boolean = true;
  accordionName: any;
  obj: any;
  public anyBooleanPropertyFromComponent:true;
  public indextestsate;
  public indextestcity;

  public countries;
  public states;
  public cities;
  public wlCountry = "country";
  public wlState = "state";
  public wlCity = "city";
  public WlName;
  public Wlcode;
  public Wldescription;
  public Wladdress;
  public WlzipCode;
  public lng: number;
  public lat: number;
  public Wlsearch:number;
  public WlFirstName;
  pager: any = {};
public country:null;
public state:null;
public city:null;
  @ViewChild("search")
  public searchElementRef: ElementRef;
  // paged items
  pagedItems: any[];
  emailPattern = "^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$";
  mobnumPattern = "^((\\+91-?)|0)?[0-9]{10}$"; 

  constructor(private router: Router, private fb: FormBuilder, private data: CreateTrailerService,
     private httpService: HttpClient,private mapsAPILoader: MapsAPILoader,
     private ngZone: NgZone,
     private activatedRoute: ActivatedRoute
     ,private toastr: ToastrService) {
       this.countries=country;
       this.states=state;
       this.cities=cities;
    
  
  }

  @Output()
  click = new EventEmitter();
  public searchControl: FormControl;


  ngOnInit() {

   // initial center position for the map
      this.lat = 12.9783;
      this.lng= 77.6647;
 
    this.createForm();

    




  }
  groups = [

    {
      title: 'Trailer company details',
      content: 'Trailer'
    },
    {
      title: 'Address info',
      content: 'Address'
    },
    {
      title: 'Contact details',
      content: 'Contact'
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
private setCurrentPosition() {
  if ("geolocation" in navigator) {
    navigator.geolocation.getCurrentPosition((position) => {
      this.lat= position.coords.latitude;
      this.lng = position.coords.longitude;
      this.zoom = 12;
    });
  }
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
    this.wlCity = cityid;
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


  createForm() {

    this.user = this.fb.group({
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
      ])
    });
    if (this.isEmpty(this.data.trailerEdit)) {

      this.user = this.fb.group({
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
        ])

  
      });

    }else{
            
      this.user = this.fb.group({
        name: [this.data.trailerEdit.name, [Validators.required, Validators.minLength(2)]],
      code: [this.data.trailerEdit.code, [Validators.required]],
      description: [this.data.trailerEdit.description],
      file:[this.data.trailerEdit.file],
      address: [this.data.trailerEdit.address, Validators.required],
      country:  [this.data.trailerEdit.country, Validators.required],
      state: [this.data.trailerEdit.state, Validators.required],
      city: [this.data.trailerEdit.city, Validators.required],
      zipcode: [this.data.trailerEdit.zipcode, Validators.required],
      longitude: [this.data.trailerEdit.longitude],
      latitude: [this.data.trailerEdit.latitude],
      search: [''],
        itemRows: this.fb.array(this.patchForm(this.data.trailerEdit.itemRows))
        
      });
      this.indextest = _.findIndex(this.countries,this.data.trailerEdit.country);
      this.indextestsate = _.findIndex(this.states,this.data.trailerEdit.state);
      this.indextestcity = _.findIndex(this.cities,this.data.trailerEdit.city);
    this.user.controls['country'].setValue(this.countries[this.indextest], {onlySelf: true});
    this.onCountrySelect(this.data.trailerEdit.country);
    this.onSateSelect(this.data.trailerEdit.state);
     this.user.controls['state'].setValue(this.states[this.indextestsate], {onlySelf: true});
     console.log(this.data.trailerEdit.file);
     this.url = this.data.trailerEdit.file;

  
  this.user.controls['city'].setValue(this.cities[this.indextestcity], {onlySelf: true});  
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
    console.log(event);

    if (event.target.files && event.target.files[0])
     {
      if (!this.validateFile(event.target.files[0].name)) {
        this.user.controls['file'].setValue('', {onlySelf: true});
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
        this.user.controls['file'].setValue('', {onlySelf: true});
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
         this.user.patchValue({
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
  
  patchForm(data) {
       return data.map(ctrl => {
      return this.fb.group({
        firstname: [ctrl.firstname],
        lastname: [ctrl.lastname],
        email: [ctrl.email],
        phone:[ctrl.phone]
      });
    });  
  }


  initItemRows() {
     
    return  this.fb.group({
        'firstname': [''],
        'lastname': [''],
        'email': ['', [Validators.pattern(this.emailPattern)]],
        'phone':['', [Validators.pattern(this.mobnumPattern)]]
     })
}

addNewRow() {
    const control = <FormArray>this.user.controls['itemRows'];
    control.push(this.initItemRows());
}

deleteRow(index: number) {
    const control = <FormArray>this.user.controls['itemRows'];
    control.removeAt(index);
}
cancelEdit() {
    this.data.trailerEdit = [];
    this.user.reset();
    this.router.navigate(['admin/trailercompany/view']);


  }
  isEmpty(obj) {
    for (var key in obj) {
      if (obj.hasOwnProperty(key))
        return false;
    }
    return true;
  }


  Submit({ value, valid }: { value: CreateTrailerService, valid: boolean }) {

    console.log(value);
    this.data.trailerEdit=value;
    this.toastr.info('Successfully created');

    this.router.navigate(['admin/trailercompany']);
    this.user.reset();

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
    console.log($event.coords.lat,$event.coords.lng); */
    this.user.controls['latitude'].setValue( $event.coords.lat, {onlySelf: true});
    this.user.controls['longitude'].setValue( $event.coords.lng, {onlySelf: true});
    this.lat = $event.coords.lat;
    this.lng = $event.coords.lng;
  }

  markerDragEnd(m: marker, $event: MouseEvent) {
    console.log('dragEnd', m, $event);
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
 interface BreadCrumb {
  label: string;
  url: string;
};

