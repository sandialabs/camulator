import { Component, effect, inject, WritableSignal, signal } from '@angular/core';
import { RTSPFormComponent } from '../rtspform/rtspform.component';
import { ButtonModule } from 'primeng/button';
import { RTSP } from '../shared/interfaces/rtsp';
import ApiService from '../api.service';
import { InputTextModule } from 'primeng/inputtext';
import { InputGroupModule } from 'primeng/inputgroup';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { Settings } from '../shared/interfaces/settings';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InputNumber } from "primeng/inputnumber";
import { DialogModule } from "primeng/dialog";
import { ToolbarModule } from "primeng/toolbar";
import { BaseDirectoryComponent } from "../base-directory/base-directory.component";
import { CardModule } from 'primeng/card';
import { PanelModule } from 'primeng/panel';
import { BadgeModule } from 'primeng/badge';
import { AboutComponent } from "../about/about.component";

@Component({
  selector: 'app-home',
  imports: [
    FormsModule,
    RTSPFormComponent,
    ButtonModule,
    InputTextModule,
    InputGroupModule,
    FloatLabelModule,
    InputGroupAddonModule,
    InputNumber,
    DialogModule,
    ToolbarModule,
    BaseDirectoryComponent,
    CardModule,
    PanelModule,
    BadgeModule,
    AboutComponent
],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export default class HomeComponent {
  private apiService = inject(ApiService);

  settings: WritableSignal<Settings | undefined> = signal<Settings | undefined>(
    undefined
  );

  startingPort: number;

  startingPortDialogVisible: boolean = false;

  constructor() {
    this.startingPort = 6000;

    this.apiService.settings$
      .pipe(takeUntilDestroyed())
      .subscribe((settings) => this.settings.set(settings));
    this.apiService.getSettings();
  }

  onAddNewCamera() {
    this.settings()?.addNewStream();
  }

  onSubmitted($event: RTSP) {
    const settings = this.settings();
    if (!settings) return;
    let index = settings.Streams.findIndex((s) => s.id == $event.id);
    if (index && index >= 0) settings.Streams[index] = $event;

    this.submit();
  }

  submit() {
    const settings = this.settings();
    if (settings) this.apiService.postSettings(settings);
  }

  onStartingPortQuestion() {
    this.startingPortDialogVisible = true;
  }

  onDeleteStream(cameraID: number) {
    const settings = this.settings();
    if (!settings) return;
    let index = settings.Streams.findIndex((s) => s.id == cameraID);
    if (index >= 0) settings.Streams.splice(index, 1);
    else console.error(`Unable to find RTSP ${cameraID}`);
  }
}
