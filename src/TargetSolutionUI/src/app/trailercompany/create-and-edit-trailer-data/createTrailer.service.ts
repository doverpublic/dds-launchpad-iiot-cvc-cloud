import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';


@Injectable()
export class CreateTrailerService {

  constructor() { }
  name: string;
  code: string;
  description: string;
  file:string;
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
 
   public storage: any;
public trailerEdit:any;
private messageSource = new BehaviorSubject('default message');
currentMessage = this.messageSource.asObservable();     
}
