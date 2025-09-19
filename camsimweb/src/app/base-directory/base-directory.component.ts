import { Component, inject, signal, WritableSignal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { InputTextModule } from 'primeng/inputtext';
import { AccordionModule } from 'primeng/accordion';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import ApiService from '../api.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DialogModule } from 'primeng/dialog';
import { FormsModule } from '@angular/forms';
import { IStreamData } from '../shared/interfaces/streamdata';
import { Settings } from '../shared/interfaces/settings';
import { PanelModule } from 'primeng/panel';

@Component({
  selector: 'app-base-directory',
  imports: [
    InputTextModule,
    InputGroupModule,
    InputGroupAddonModule,
    FloatLabelModule,
    ButtonModule,
    DialogModule,
    FormsModule,
    AccordionModule,
    TableModule,
    CardModule,
    PanelModule,
  ],
  templateUrl: './base-directory.component.html',
  styleUrl: './base-directory.component.css',
})
export class BaseDirectoryComponent {
  private apiService = inject(ApiService);

  files: WritableSignal<IStreamData[]> = signal<IStreamData[]>([]);

  baseDirectory: string;
  baseDirectoryDialogVisible: boolean = false;

  settings: WritableSignal<Settings | undefined> = signal<Settings | undefined>(
    undefined
  );

  constructor() {
    this.baseDirectory = '';

    this.apiService.settings$
      .pipe(takeUntilDestroyed())
      .subscribe((settings) => {
        this.settings.set(settings);
        if (settings?.BaseDirectory) {
          this.baseDirectory = settings.BaseDirectory;
          this.onGetFilesInFolder();
        }
      });

    this.apiService.baseDirectoryFiles$
      .pipe(takeUntilDestroyed())
      .subscribe((files) => this.files.set(files));
  }

  addStream(stream: IStreamData) {
    this.settings()?.addNewFileStream(stream);
  }

  onGetFilesInFolder() {
    if (!this.baseDirectory) return;
    this.apiService.getBaseDirectoryFiles(this.baseDirectory);
  }

  onGetFilesQuestion() {
    this.baseDirectoryDialogVisible = true;
  }
}
