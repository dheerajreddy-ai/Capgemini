import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TeacherService } from '../teacher.service';
import { ToastService } from '../../../core/services/toast.service';
import { ClassSection } from '../../../core/models/models';

@Component({
  selector: 'ev-teacher-homework',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-header mb-4">
      <div>
        <h1 class="ev-page-title">Assign Homework</h1>
        <p class="ev-page-sub text-secondary-ev">Create a homework assignment for your class</p>
      </div>
    </div>

    <div class="ev-card p-4" style="max-width: 640px;">
      <div class="row g-3">
        <div class="col-12 col-sm-6">
          <label class="form-label">Class <span class="text-danger">*</span></label>
          <select class="form-select" [(ngModel)]="form.class">
            <option value="">Select class…</option>
            @for (c of uniqueClasses(); track c) {
              <option [value]="c">Class {{ c }}</option>
            }
          </select>
        </div>
        <div class="col-12 col-sm-6">
          <label class="form-label">Section</label>
          <select class="form-select" [(ngModel)]="form.section" [disabled]="!form.class">
            <option value="">All sections</option>
            @for (s of sectionsForClass(); track s) {
              <option [value]="s">{{ s }}</option>
            }
          </select>
        </div>
        <div class="col-12 col-sm-6">
          <label class="form-label">Subject <span class="text-danger">*</span></label>
          <select class="form-select" [(ngModel)]="form.subject">
            <option value="">Select subject…</option>
            @for (s of subjects; track s) { <option [value]="s">{{ s }}</option> }
          </select>
        </div>
        <div class="col-12 col-sm-6">
          <label class="form-label">Due Date <span class="text-danger">*</span></label>
          <input type="date" class="form-control" [(ngModel)]="form.dueDate" [min]="today" />
        </div>
        <div class="col-12">
          <label class="form-label">Description <span class="text-danger">*</span></label>
          <textarea class="form-control" rows="4" [(ngModel)]="form.description"
            placeholder="Describe the homework task in detail…"></textarea>
        </div>

        @if (error()) {
          <div class="col-12">
            <div class="alert alert-danger py-2 mb-0">{{ error() }}</div>
          </div>
        }

        <div class="col-12 d-flex justify-content-end gap-2">
          <button class="btn btn-outline-secondary" type="button" (click)="reset()">Clear</button>
          <button class="btn btn-primary px-4" (click)="submit()" [disabled]="saving()">
            @if (saving()) { <span class="spinner-border spinner-border-sm me-2"></span> }
            Assign Homework
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .ev-card { background: var(--ev-bg-card); border-radius: 14px; border: 1px solid var(--ev-border); }
  `],
})
export class TeacherHomeworkComponent implements OnInit {
  private readonly service = inject(TeacherService);
  private readonly toast = inject(ToastService);

  readonly today = new Date().toISOString().split('T')[0];

  readonly subjects = ['Mathematics', 'Science', 'English', 'Telugu', 'Social Studies', 'Hindi', 'Computer Science', 'Other'];

  form = { class: '', section: '', subject: '', description: '', dueDate: '' };

  readonly classSections = signal<ClassSection[]>([]);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly uniqueClasses = computed(() => [...new Set(this.classSections().map((c) => c.class))].sort());

  sectionsForClass(): string[] {
    return this.classSections()
      .filter((c) => c.class === this.form.class && c.section)
      .map((c) => c.section!);
  }

  ngOnInit(): void {
    this.service.getClassSections().subscribe({ next: (cs) => this.classSections.set(cs) });
  }

  reset(): void {
    this.form = { class: '', section: '', subject: '', description: '', dueDate: '' };
    this.error.set(null);
  }

  submit(): void {
    this.error.set(null);
    if (!this.form.class) { this.error.set('Please select a class.'); return; }
    if (!this.form.subject) { this.error.set('Please select a subject.'); return; }
    if (!this.form.description.trim()) { this.error.set('Please enter a description.'); return; }
    if (!this.form.dueDate) { this.error.set('Please set a due date.'); return; }

    this.saving.set(true);
    this.service.assignHomework({
      class: this.form.class,
      section: this.form.section || undefined,
      subject: this.form.subject,
      description: this.form.description.trim(),
      dueDate: new Date(this.form.dueDate).toISOString(),
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Homework assigned', `${this.form.subject} homework sent to Class ${this.form.class}${this.form.section ? ' – ' + this.form.section : ''}.`);
        this.reset();
      },
      error: () => { this.saving.set(false); this.toast.error('Error', 'Failed to assign homework.'); },
    });
  }
}
