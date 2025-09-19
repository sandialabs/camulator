import { Injectable, Signal, WritableSignal, inject, signal } from '@angular/core';
import { ISettings, Settings } from './shared/interfaces/settings';
import { AppConfigService } from './app-config.service';
import { HttpClient } from '@angular/common/http';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of } from 'rxjs';
import { IStreamData } from './shared/interfaces/streamdata';

@Injectable({
  providedIn: 'root',
})
export default class ApiService {
  private http = inject(HttpClient);
  private settings = inject(AppConfigService);

  #settings: WritableSignal<Settings | undefined> = signal(undefined);
  #baseDirectoryFiles: WritableSignal<IStreamData[]> = signal([]);

  settings$ = toObservable(this.#settings);
  baseDirectoryFiles$ = toObservable(this.#baseDirectoryFiles);

  constructor() {}

  getSettings(): void {
    this.http
      .get<ISettings>(this.settings.getBaseURL() + 'api/settings')
      .pipe(
        catchError((error) => {
          console.error('Error retrieving settings', error);
          return of(undefined);
        }))
      .subscribe((settings) => {
        if(settings)
          this.#settings.set(new Settings(settings));
      }
    );
  }

  postSettings(settings: Settings | undefined): void {
    if(!settings)
      return;

    const iSettings: ISettings = settings.asISettings();

    this.http.post<ISettings>(
      this.settings.getBaseURL() + 'api/settings', iSettings)
      .pipe(
        catchError((error) => {
          console.error('Error posting settings', error, iSettings);
          return of(undefined);
        })
      )
      .subscribe((settings) => {
        if(settings)
          this.#settings.set(new Settings(settings));
      })
  }

  getBaseDirectoryFiles(directory: string): void {
    this.http
      .get<IStreamData[]>(this.settings.getBaseURL() + 'api/basedirectory', {
        params: {
          basedirectory: directory,
        },
      })
      .pipe(
        catchError((error) => {
          console.error('Error retrieving files', error);
          return of([]);
        })
      )
      .subscribe((files) => {
        this.#baseDirectoryFiles.set(files);
      }
    );
  }
}
