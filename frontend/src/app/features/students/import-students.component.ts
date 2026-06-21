import { Component, ChangeDetectionStrategy, inject, input, output, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { StudentsService } from '../../core/services/students.service';
import { ToastService } from '../../core/services/toast.service';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';

@Component({
  selector: 'ev-import-students',
  standalone: true,
  imports: [DrawerComponent, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-drawer [open]="open()" title="Import Students" subtitle="Upload an Excel (.xlsx) file" (close)="reset(); close.emit()">
      <div class="ev-dropzone" [class.is-drag]="dragging()"
           (dragover)="$event.preventDefault(); dragging.set(true)"
           (dragleave)="dragging.set(false)"
           (drop)="onDrop($event)"
           (click)="fileInput.click()">
        <input #fileInput type="file" accept=".xlsx,.xls" hidden (change)="onPick($any($event.target).files)" />
        <div class="ev-dropzone__icon"><i class="bi bi-cloud-arrow-up"></i></div>
        @if (file()) {
          <div class="fw-bold">{{ file()!.name }}</div>
          <small class="text-secondary-ev">{{ (file()!.size / 1024) | number:'1.0-0' }} KB · click to replace</small>
        } @else {
          <div class="fw-bold">Drag & drop your file here</div>
          <small class="text-secondary-ev">or click to browse — .xlsx up to 5MB</small>
        }
      </div>

      <div class="ev-hint mt-3">
        <i class="bi bi-info-circle me-2"></i>
        Required columns: <b>StudentCode, FullName, Class, Section, ParentName, ParentPhone, FeesDue, FeesDueDate</b>
      </div>

      @if (result(); as r) {
        <div class="ev-import-result mt-3">
          <div class="d-flex gap-3">
            <div class="ev-import-stat text-success"><b>{{ r.imported }}</b><span>Imported</span></div>
            <div class="ev-import-stat text-danger"><b>{{ r.failed }}</b><span>Failed</span></div>
          </div>
          @if (r.errors.length) {
            <ul class="ev-error-list mt-2">
              @for (e of r.errors; track e) { <li>{{ e }}</li> }
            </ul>
          }
        </div>
      }

      <div slot="footer">
        <button class="btn btn-soft" (click)="reset(); close.emit()">Close</button>
        <button class="btn btn-primary" [disabled]="!file() || importing()" (click)="doImport()">
          @if (importing()) { <span class="spinner-border spinner-border-sm me-2"></span>Importing… }
          @else { Import students }
        </button>
      </div>
    </ev-drawer>
  `,
  styles: [`
    .ev-dropzone { border: 2px dashed var(--ev-border-strong); border-radius: 18px; padding: 2.5rem 1.5rem; text-align: center; cursor: pointer; transition: all .2s ease; background: var(--ev-surface-2); }
    .ev-dropzone:hover, .ev-dropzone.is-drag { border-color: var(--ev-primary); background: var(--ev-primary-soft); }
    .ev-dropzone__icon { width: 60px; height: 60px; margin: 0 auto 1rem; border-radius: 16px; display: grid; place-items: center; font-size: 1.6rem; background: var(--ev-primary-soft); color: var(--ev-primary); }
    .ev-hint { background: var(--ev-info-soft); color: #4338CA; border-radius: 12px; padding: .75rem .9rem; font-size: .82rem; }
    .ev-import-stat b { font-family: "Sora",sans-serif; font-size: 1.6rem; display: block; line-height: 1; }
    .ev-import-stat span { font-size: .78rem; color: var(--ev-text-secondary); }
    .ev-error-list { font-size: .82rem; color: var(--ev-danger); padding-left: 1.1rem; max-height: 160px; overflow-y: auto; }
  `],
})
export class ImportStudentsComponent {
  private readonly service = inject(StudentsService);
  private readonly toast = inject(ToastService);

  readonly open = input(false);
  readonly close = output<void>();
  readonly imported = output<void>();

  readonly dragging = signal(false);
  readonly file = signal<File | null>(null);
  readonly importing = signal(false);
  readonly result = signal<{ imported: number; failed: number; errors: string[] } | null>(null);

  onDrop(e: DragEvent): void {
    e.preventDefault();
    this.dragging.set(false);
    this.onPick(e.dataTransfer?.files ?? null);
  }

  onPick(files: FileList | null): void {
    if (files?.length) { this.file.set(files[0]); this.result.set(null); }
  }

  doImport(): void {
    if (!this.file()) return;
    this.importing.set(true);
    this.service.import(this.file()!).subscribe({
      next: (r) => {
        this.importing.set(false);
        this.result.set(r);
        this.toast.success('Import complete', `${r.imported} added, ${r.failed} failed.`);
        if (r.imported > 0) this.imported.emit();
      },
      error: () => this.importing.set(false),
    });
  }

  reset(): void { this.file.set(null); this.result.set(null); }
}
