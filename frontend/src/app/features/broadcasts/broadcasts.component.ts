import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BroadcastsService, CreateBroadcastRequest } from '../../core/services/broadcasts.service';
import { Broadcast, BroadcastMediaType } from '../../core/models/models';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'ev-broadcasts',
  standalone: true,
  imports: [FormsModule, RelativeTimePipe, SkeletonComponent, EmptyStateComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Broadcasts</h1>
        <p class="ev-page-sub">Send WhatsApp messages & media to all parents instantly</p>
      </div>
      <button class="btn btn-primary" (click)="showCompose.set(true)">
        <i class="bi bi-broadcast me-2"></i>New Broadcast
      </button>
    </div>

    <!-- Compose Modal -->
    @if (showCompose()) {
      <div class="modal show d-block" style="background:rgba(0,0,0,.5)">
        <div class="modal-dialog modal-lg modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="bi bi-broadcast me-2 text-primary"></i>New Broadcast</h5>
              <button class="btn-close" (click)="cancelCompose()"></button>
            </div>
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label fw-semibold">Title <span class="text-danger">*</span></label>
                <input class="form-control" [(ngModel)]="form.title" placeholder="e.g. Sports Day Notice, Holiday Announcement" />
              </div>
              <div class="mb-3">
                <label class="form-label fw-semibold">Message <span class="text-danger">*</span></label>
                <textarea class="form-control" rows="5" [(ngModel)]="form.message"
                  placeholder="Type your message in Telugu or English..."></textarea>
                <div class="form-text">{{ form.message.length }}/4096 characters</div>
              </div>
              <div class="row g-3 mb-3">
                <div class="col-md-4">
                  <label class="form-label fw-semibold">Target Class</label>
                  <input class="form-control" [(ngModel)]="form.targetClass" placeholder="All classes" />
                </div>
                <div class="col-md-4">
                  <label class="form-label fw-semibold">Target Section</label>
                  <input class="form-control" [(ngModel)]="form.targetSection" placeholder="All sections" />
                </div>
                <div class="col-md-4">
                  <label class="form-label fw-semibold">Media Type</label>
                  <select class="form-select" [(ngModel)]="form.mediaType">
                    <option value="None">No media</option>
                    <option value="Image">Image</option>
                    <option value="Document">Document (PDF)</option>
                    <option value="Video">Video</option>
                  </select>
                </div>
              </div>
              @if (form.mediaType !== 'None') {
                <div class="mb-3">
                  <label class="form-label fw-semibold">Media URL <span class="text-danger">*</span></label>
                  <input class="form-control" [(ngModel)]="form.mediaUrl"
                    placeholder="https://example.com/notice.pdf — must be publicly accessible" />
                  <div class="form-text text-warning"><i class="bi bi-info-circle me-1"></i>URL must be publicly accessible for Twilio to deliver it.</div>
                </div>
              }
              <div class="alert alert-info py-2 small mb-0">
                <i class="bi bi-info-circle me-2"></i>
                Messages are sent to all registered parents (excluding Do-Not-Call). Rate-limited at ~1/sec.
                @if (form.targetClass) { Filtered to class <strong>{{ form.targetClass }}</strong>. }
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn btn-outline-secondary" (click)="cancelCompose()">Cancel</button>
              <button class="btn btn-primary" [disabled]="!isFormValid() || sending()" (click)="send()">
                @if (sending()) { <span class="spinner-border spinner-border-sm me-2"></span> }
                <i class="bi bi-send me-2"></i>Send Now
              </button>
            </div>
          </div>
        </div>
      </div>
    }

    <!-- List -->
    @if (loading()) {
      <div class="ev-card ev-card-pad"><ev-skeleton [rows]="5" [height]="20" /></div>
    } @else if (broadcasts().length === 0) {
      <div class="ev-card">
        <ev-empty-state icon="bi-broadcast" title="No broadcasts yet"
          message="Send your first broadcast to reach all parents instantly via WhatsApp." />
      </div>
    } @else {
      <div class="ev-card">
        <table class="table table-hover mb-0">
          <thead class="table-light">
            <tr>
              <th>Title</th>
              <th>Target</th>
              <th class="text-center">Recipients</th>
              <th class="text-center">Delivered</th>
              <th class="text-center">Failed</th>
              <th>Status</th>
              <th>Sent</th>
            </tr>
          </thead>
          <tbody>
            @for (b of broadcasts(); track b.id) {
              <tr>
                <td>
                  <div class="fw-semibold">{{ b.title }}</div>
                  <div class="text-muted small text-truncate" style="max-width:260px">{{ b.message }}</div>
                  @if (b.mediaType !== 'None') {
                    <span class="badge bg-light text-dark border mt-1">
                      <i class="bi me-1" [class.bi-image]="b.mediaType === 'Image'"
                         [class.bi-file-pdf]="b.mediaType === 'Document'"
                         [class.bi-camera-video]="b.mediaType === 'Video'"></i>{{ b.mediaType }}
                    </span>
                  }
                </td>
                <td class="small">
                  @if (b.targetClass) { Class {{ b.targetClass }}{{ b.targetSection ? ' ' + b.targetSection : '' }} }
                  @else { <span class="text-muted">All parents</span> }
                </td>
                <td class="text-center fw-bold">{{ b.totalRecipients }}</td>
                <td class="text-center text-success fw-bold">{{ b.sentCount }}</td>
                <td class="text-center text-danger fw-bold">{{ b.failedCount }}</td>
                <td><ev-status-badge [value]="b.status" /></td>
                <td class="text-muted small">
                  @if (b.sentAt) { {{ b.sentAt | evRelativeTime }} }
                  @else { {{ b.createdAt | evRelativeTime }} }
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
})
export class BroadcastsComponent implements OnInit {
  private readonly service = inject(BroadcastsService);

  readonly loading = signal(true);
  readonly broadcasts = signal<Broadcast[]>([]);
  readonly showCompose = signal(false);
  readonly sending = signal(false);
  readonly error = signal('');

  form: CreateBroadcastRequest = this.emptyForm();

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.list().subscribe({
      next: (r) => { this.broadcasts.set(r.items ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  isFormValid(): boolean {
    return !!this.form.title.trim() && !!this.form.message.trim()
      && (this.form.mediaType === 'None' || !!this.form.mediaUrl?.trim());
  }

  send(): void {
    if (!this.isFormValid()) return;
    this.sending.set(true);
    this.service.create(this.form).subscribe({
      next: () => { this.sending.set(false); this.showCompose.set(false); this.form = this.emptyForm(); this.load(); },
      error: () => { this.sending.set(false); },
    });
  }

  cancelCompose(): void { this.showCompose.set(false); this.form = this.emptyForm(); }

  private emptyForm(): CreateBroadcastRequest {
    return { title: '', message: '', mediaType: 'None', sendNow: true };
  }
}
