import { Injectable } from '@angular/core';
import { UserAgentApplication } from 'msal';
import { AppConfig } from '../../app.config';


@Injectable()
export class MSALService {

    public access_token = null;
    private app: any;
    public user;
    public isAuthenticated = false;
    private applicationConfig: any = {
        clientID: this.configData.getConfig('clientId'),
        authority: 'https://login.microsoftonline.com/tfp/'+this.configData.getConfig('tenant')+'/'+this.configData.getConfig('signinPolicy')+'',
        b2cScopes: [this.configData.getConfig('b2cScopes')],
        redirectUrl: this.configData.getConfig('redirectUrl')
    };

    constructor(private configData: AppConfig) {

        this.app = new UserAgentApplication(
            this.applicationConfig.clientID,
            this.applicationConfig.authority,
            (errorDesc, token, error, tokenType) => {
            // callback for login redirect
            if (error) {
               
                return;
            }
            console.log('Callback for login');
            this.access_token = token;
        });
        this.app.redirectUri = this.applicationConfig.redirectUrl;
    }

    public login() {

        return this.app.loginRedirect(this.applicationConfig.b2cScopes).then(
            idToken => {
                this.app.acquireTokenSilent(this.applicationConfig.b2cScopes).then(
                    accessToken => {
                        this.access_token = accessToken;
                        this.user = this.app.getUser(); // AZURE AD
                        this.isAuthenticated = true;
                    },
                    error => {
                        this.app.acquireTokenPopup(this.applicationConfig.b2cScopes).then(accessToken => {
                            console.log('Error acquiring the popup:\n' + error);
                        });
                    }
                );
            },
            error => {
                console.log('Error during login:\n' + error);
            }
        );
    }

    public logout() {
       this.app.logout();
 }

    public isOnline(): boolean {
        return this.app.getUser() != null;
    }

    public getUser() {
        const user = this.app.getUser();
        return user;
    }

    public getToken() {
        return this.app.acquireTokenSilent(this.applicationConfig.b2cScopes).then(accessToken => {
            return accessToken;
        },
        error => {
            return this.app.acquireTokenSilent(this.applicationConfig.b2cScopes).then(accessToken => {
                return accessToken;
            },
            err => {
                console.error(err);
            });
        });
    }

    public getTokenId() {
        if(this.access_token) {
            return this.access_token;
        }else {
            return sessionStorage.getItem('msal.idtoken');
        }
    }
}
