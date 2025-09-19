import { Component } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import packageJson from '../../../package.json';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { PanelModule } from 'primeng/panel';

export interface IPackage {
  name: string;
  link: string;
}

@Component({
  selector: 'app-about',
  imports: [
    DialogModule,
    TableModule,
    ToolbarModule,
    ButtonModule,
    DividerModule,
    PanelModule,
  ],
  templateUrl: './about.component.html',
  styleUrl: './about.component.css'
})
export class AboutComponent {
  packages: IPackage[] = [
    {
      name: "Angular",
      link: "https://angular.dev"
    },
    {
      name: "JSON.NET",
      link: "https://www.newtonsoft.com/json",
    },
    {
      name: "LibVLCSharp",
      link: "https://github.com/videolan/libvlcsharp",
    },
    {
      name: "PrimeNG",
      link: "https://primeng.org",
    },
    {
      name: "RxJS",
      link: "https://rxjs.dev",
    },
  ];
  displayDialog: boolean = false;
  version: string;

  constructor() {
    this.version = packageJson.version;
  }

  showAbout() {
    this.displayDialog = true;
  }
}
