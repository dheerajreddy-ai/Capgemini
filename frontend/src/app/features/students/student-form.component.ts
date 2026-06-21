import { Component, ChangeDetectionStrategy, inject, input, output, effect, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { StudentsService } from '../../core/services/students.service';
import { ToastService } from '../../core/services/toast.service';
import { Student } from '../../core/models/models';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';

@Component({
  selector: 'ev-student-form',
  standalone: true,
  imports: [ReactiveFormsModule, DrawerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-drawer [open]="open()" size="lg"
      [title]="student() ? 'Edit Student' : 'Add Student'"
      [subtitle]="student() ? student()!.fullName : 'Create a new student record'"
      (close)="close.emit()">
      <form [formGroup]="form" id="studentForm" (ngSubmit)="submit()">
        <h6 class="ev-form-section">Basic details</h6>
        <div class="row g-3 mb-4">
          <div class="col-md-6"><label class="form-label">Full name</label><input class="form-control" formControlName="fullName" /></div>
          <div class="col-md-6"><label class="form-label">Student code</label><input class="form-control" formControlName="studentCode" /></div>
          <div class="col-md-6"><label class="form-label">Class</label><input class="form-control" formControlName="class" placeholder="10th" /></div>
          <div class="col-md-6"><label class="form-label">Section</label><input class="form-control" formControlName="section" placeholder="A" /></div>
        </div>

        <h6 class="ev-form-section">Parent / guardian</h6>
        <div class="row g-3 mb-4">
          <div class="col-md-6"><label class="form-label">Parent name</label><input class="form-control" formControlName="parentName" /></div>
          <div class="col-md-6"><label class="form-label">Parent phone</label><input class="form-control" formControlName="parentPhone" placeholder="9876543210" /></div>
          <div class="col-md-6"><label class="form-label">Alternate phone</label><input class="form-control" formControlName="parentPhone2" /></div>
          <div class="col-md-6"><label class="form-label">WhatsApp</label><input class="form-control" formControlName="parentWhatsApp" /></div>
        </div>

        <h6 class="ev-form-section">Fees</h6>
        <div class="row g-3 mb-4">
          <div class="col-md-4"><label class="form-label">Fees due (₹)</label><input type="number" class="form-control" formControlName="feesDue" /></div>
          <div class="col-md-4"><label class="form-label">Due date</label><input type="date" class="form-control" formControlName="feesDueDate" /></div>
          <div class="col-md-4">
            <label class="form-label">Status</label>
            <select class="form-select" formControlName="feesStatus">
              <option>Paid</option><option>Partial</option><option>Unpaid</option><option>Overdue</option>
            </select>
          </div>
        </div>

        <h6 class="ev-form-section">Marks & attendance</h6>
        <div class="row g-3">
          <div class="col-6 col-md-4"><label class="form-label">Maths %</label><input type="number" class="form-control" formControlName="mathsMarks" /></div>
          <div class="col-6 col-md-4"><label class="form-label">Science %</label><input type="number" class="form-control" formControlName="scienceMarks" /></div>
          <div class="col-6 col-md-4"><label class="form-label">English %</label><input type="number" class="form-control" formControlName="englishMarks" /></div>
          <div class="col-6 col-md-4"><label class="form-label">Telugu %</label><input type="number" class="form-control" formControlName="teluguMarks" /></div>
          <div class="col-6 col-md-4"><label class="form-label">Hindi %</label><input type="number" class="form-control" formControlName="hindiMarks" /></div>
          <div class="col-6 col-md-4"><label class="form-label">Social %</label><input type="number" class="form-control" formControlName="socialMarks" /></div>
          <div class="col-12 col-md-6"><label class="form-label">Attendance %</label><input type="number" class="form-control" formControlName="attendance" /></div>
        </div>

        <div class="mt-3"><label class="form-label">Notes</label><textarea class="form-control" rows="2" formControlName="notes"></textarea></div>
      </form>

      <div slot="footer">
        <button class="btn btn-soft" (click)="close.emit()">Cancel</button>
        <button class="btn btn-primary" form="studentForm" type="submit" [disabled]="saving()">
          @if (saving()) { <span class="spinner-border spinner-border-sm me-2"></span> }
          {{ student() ? 'Save changes' : 'Create student' }}
        </button>
      </div>
    </ev-drawer>
  `,
  styles: [`.ev-form-section { font-size: .8rem; text-transform: uppercase; letter-spacing: .04em; color: var(--ev-text-secondary); font-weight: 700; margin-bottom: .75rem; }`],
})
export class StudentFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(StudentsService);
  private readonly toast = inject(ToastService);

  readonly open = input(false);
  readonly student = input<Student | null>(null);
  readonly close = output<void>();
  readonly saved = output<void>();

  readonly saving = signal(false);

  readonly form = this.fb.group({
    fullName: ['', Validators.required],
    studentCode: [''],
    class: ['', Validators.required],
    section: [''],
    parentName: ['', Validators.required],
    parentPhone: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
    parentPhone2: [''],
    parentWhatsApp: [''],
    feesDue: [0],
    feesDueDate: [''],
    feesStatus: ['Unpaid'],
    mathsMarks: [null as number | null],
    scienceMarks: [null as number | null],
    englishMarks: [null as number | null],
    teluguMarks: [null as number | null],
    hindiMarks: [null as number | null],
    socialMarks: [null as number | null],
    attendance: [null as number | null],
    notes: [''],
  });

  constructor() {
    effect(() => {
      const s = this.student();
      if (s) this.form.patchValue({ ...s, feesDueDate: s.feesDueDate?.split('T')[0] ?? '' });
      else this.form.reset({ feesDue: 0, feesStatus: 'Unpaid' });
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); this.toast.warning('Check the form', 'Some fields need attention.'); return; }
    this.saving.set(true);
    const payload = this.form.value as Partial<Student>;
    const req = this.student()
      ? this.service.update(this.student()!.id, payload)
      : this.service.create(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.saved.emit(); },
      error: () => this.saving.set(false),
    });
  }
}
