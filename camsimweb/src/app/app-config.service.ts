import { Injectable, inject } from '@angular/core';
import { IConfig } from './shared/interfaces/config';
import * as config from "../assets/config.json";


@Injectable({
  providedIn: 'root'
})
export class AppConfigService {
  private config: IConfig | null = config;

  constructor() {
    console.log("AppConfigService:", this.config);
  }

  getBaseURL(): string | null {
    if(this.config == null)
      return null;

    const url: string = this.config.apiBaseURL;
    const port: number = this.config.port;
    const baseUrl: string = `${url}:${port}/`;

    console.log(baseUrl);

    return baseUrl;
  }
}
