import { ChangeDetectorRef, Component, computed, effect, inject, input, model, output, signal, WritableSignal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { RTSP } from '../shared/interfaces/rtsp';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputTextModule } from 'primeng/inputtext';
import { SelectButtonChangeEvent, SelectButtonModule } from 'primeng/selectbutton';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { DataViewModule } from 'primeng/dataview';
import { EValidity, Settings } from '../shared/interfaces/settings';
import { MessageModule } from "primeng/message";
import { ConfirmationService } from 'primeng/api';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { SelectModule } from 'primeng/select';
import { DialogModule } from 'primeng/dialog';
import ApiService from '../api.service';
// import { AsyncPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { IStreamData } from '../shared/interfaces/streamdata';

interface ISelect<T> {
  label: string,
  value: T,
};
@Component({
  selector: 'app-rtspform',
  imports: [
    FormsModule,
    ButtonModule,
    InputNumberModule,
    InputTextModule,
    FloatLabelModule,
    InputGroupModule,
    InputGroupAddonModule,
    ReactiveFormsModule,
    SelectButtonModule,
    DataViewModule,
    MessageModule,
    ConfirmPopupModule,
    SelectModule,
    DialogModule,
    // AsyncPipe,
  ],
  providers: [ConfirmationService],
  templateUrl: './rtspform.component.html',
  styleUrl: './rtspform.component.css',
})
export class RTSPFormComponent {
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);
  private confirm = inject(ConfirmationService);
  apiService = inject(ApiService);

  rtsp = input.required<RTSP>();
  settings = input<Settings>();
  // streamFiles = input<string[]>();
  rtspSubmitted = output<RTSP>();
  delete = output<boolean>();

  streamFiles: WritableSignal<IStreamData[]> = signal<IStreamData[]>([]);

  theRTSP: RTSP = RTSP.Default();
  options: ISelect<string>[] = [
    { label: 'File', value: 'file' },
    { label: 'Text', value: 'text' },
  ];
  optionsValue: 'file' | 'text' = 'file';
  message: string | null = null;
  // files: string[] = [];
  // validFPS: ISelect<number>[];

  fileStreamDialogVisible: boolean = false;
  textStreamDialogVisible: boolean = false;

  constructor() {
    effect(() => {
      // console.log("effect:", this.rtsp());
      this.theRTSP = this.rtsp();
      this.optionsValue = this.theRTSP.File === null ? 'text' : 'file';
    });

    effect(() => {
      this.apiService.baseDirectoryFiles$
      // .pipe(takeUntilDestroyed())
      .subscribe(files => this.streamFiles.set(files));
    });

    // this.validFPS = RTSP.validFPS.map((fps) => <ISelect<number>>{
    //   label: fps.toString(),
    //   value: fps,
    // });
  }

  onSubmit() {}

  onPortChange($event: KeyboardEvent) {
    // console.log('onPortChange', this.theRTSP.Port);
    let isValid = this.settings()?.isValidPort(this.theRTSP, this.theRTSP.Port);

    switch (isValid) {
      case EValidity.IS_VALID:
        this.message = null;
        break;
      case EValidity.IS_INVALID:
        this.message = 'Invalid port number';
        break;
      case EValidity.IS_DUPLICATED:
        this.message = 'Duplicate port number';
        break;
    }
  }

  onFPSChange($event: KeyboardEvent) {
    this.theRTSP.fixFPS();
  }

  // onFileTextSelection(event: SelectButtonChangeEvent) {
  //   // console.log('onFileTextSelection', event, this.optionsValue);
  // }

  onFileQuestion() {
    this.fileStreamDialogVisible = true;
  }

  onTextQuestion() {
    this.textStreamDialogVisible = true;
  }

  confirmDelete($event: Event) {
    console.log('confirmDelete', $event);
    this.confirm.confirm({
      target: $event.currentTarget as EventTarget,
      message: 'Do you want to delete this stream?',
      icon: 'pi pi-info-circle',
      rejectButtonProps: {
        label: 'No',
        severity: 'secondary',
        outlined: true,
      },
      acceptButtonProps: {
        label: 'Yes',
        severity: 'danger',
      },
      accept: () => {
        this.delete.emit(true);
      },
      reject: () => {},
    });
  }
}
