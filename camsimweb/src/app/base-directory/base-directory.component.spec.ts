import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BaseDirectoryComponent } from './base-directory.component';

describe('BaseDirectoryComponent', () => {
  let component: BaseDirectoryComponent;
  let fixture: ComponentFixture<BaseDirectoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BaseDirectoryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BaseDirectoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
