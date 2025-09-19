import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RTSPFormComponent } from './rtspform.component';

describe('RTSPFormComponent', () => {
  let component: RTSPFormComponent;
  let fixture: ComponentFixture<RTSPFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RTSPFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RTSPFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
