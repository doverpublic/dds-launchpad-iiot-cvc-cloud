import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';


@Injectable()
export class CreateDispatchOfficeService {

  constructor() { }
  name: string;
  file:string;
  code: string;
  description: string;
  address:string;
  country:string;
  state:string;
  city:string;
  zipcode:string;
  longitude:number;
  latitude:number;
  search:string;
  firstname:string;
  lastname:string;
  email:string;
  phone:number;
  shiftname:string;
  shiftStartTime:number;
  shiftEndTime:number;
  timezone:number;
  
public dispatchData:any;
  
}
