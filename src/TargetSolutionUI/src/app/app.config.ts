import { Inject, Injectable } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/observable';
import 'rxjs/add/operator/map';


@Injectable()
export class AppConfig {

    private config: Object = null;
    private privacy : any;
    private terms : any;

    constructor(private http: Http) {

    }
    public getConfig(key: any) {
        return this.config[key];
    }

    public getPrivacy() {
        return this.privacy;
    }

    public getTerms() {
        return this.terms;
    }
    
    public load() {
        let headers = new Headers({ 'Content-Type': 'application/json' });
            headers.append('Cache-Control', 'no-cache');
            headers.append('Pragma', 'no-cache');
            headers.append('Expires', 'Sat, 01 Jan 2000 00:00:00 GMT');
        let options = new RequestOptions({ headers: headers });
        const promise = this.http.get("config.json", options).map(res => res.json()).toPromise();
        promise.then(keys => {
            console.log(keys);
            if(Object.keys(keys).length === 0 ) {
                alert("Configuration value should not be empty");
            }
            if(keys['ip'] == '' || keys['tenant'] == '' || keys['clientId'] == '') {
                alert("ip, tenant and clientId should not be empty");
            }
            this.config = keys;
        });
        promise.catch((ex) => {
            console.error('Error fetching users', ex);
            alert("Configuration file missing");
        });
        return promise;
    }

    public loadPrivacy() {
        let headers = new Headers({ 'Content-Type': 'application/json' });
            headers.append('Cache-Control', 'no-cache');
            headers.append('Pragma', 'no-cache');
            headers.append('Expires', 'Sat, 01 Jan 2000 00:00:00 GMT');
        let options = new RequestOptions({ headers: headers });
        const promisePrivacy = this.http.get("privacy.pdf", options).toPromise();
        promisePrivacy.then(keys => {
            this.privacy = (<any>keys).url;
        });
        promisePrivacy.catch((ex) => {
            console.error('Error fetching privacy', ex);
        });
        return promisePrivacy;
    }

    public loadTerms() {
        let headers = new Headers({ 'Content-Type': 'application/json' });
            headers.append('Cache-Control', 'no-cache');
            headers.append('Pragma', 'no-cache');
            headers.append('Expires', 'Sat, 01 Jan 2000 00:00:00 GMT');
        let options = new RequestOptions({ headers: headers });
        const promiseTerms = this.http.get("terms.pdf", options).toPromise();
        promiseTerms.then(keys => {
            this.terms = (<any>keys).url;
        });
        promiseTerms.catch((ex) => {
            console.error('Error fetching terms', ex);
        });
        return promiseTerms;
    }
}
