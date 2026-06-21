import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
    pathMatch: 'full',
    redirectTo: 'login',
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
    title: 'Sign in · EduVoice',
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component').then((m) => m.ForgotPasswordComponent),
    title: 'Forgot password · EduVoice',
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password.component').then((m) => m.ResetPasswordComponent),
    title: 'Reset password · EduVoice',
  },
  {
    path: '',
    loadComponent: () => import('./layout/main-layout/main-layout.component').then((m) => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
        title: 'Dashboard · EduVoice',
      },
      {
        path: 'students',
        loadComponent: () => import('./features/students/students.component').then((m) => m.StudentsComponent),
        title: 'Students · EduVoice',
      },
      {
        path: 'students/:id',
        loadComponent: () =>
          import('./features/students/student-detail.component').then((m) => m.StudentDetailComponent),
        title: 'Student · EduVoice',
      },
      {
        path: 'campaigns',
        loadComponent: () => import('./features/campaigns/campaigns.component').then((m) => m.CampaignsComponent),
        title: 'Campaigns · EduVoice',
      },
      {
        path: 'campaigns/:id',
        loadComponent: () =>
          import('./features/campaigns/campaign-detail.component').then((m) => m.CampaignDetailComponent),
        title: 'Campaign · EduVoice',
      },
      {
        path: 'calls',
        loadComponent: () => import('./features/calls/calls.component').then((m) => m.CallsComponent),
        title: 'Call Logs · EduVoice',
      },
      {
        path: 'complaints',
        loadComponent: () => import('./features/complaints/complaints.component').then((m) => m.ComplaintsComponent),
        title: 'Complaints · EduVoice',
      },
      {
        path: 'broadcasts',
        loadComponent: () => import('./features/broadcasts/broadcasts.component').then((m) => m.BroadcastsComponent),
        title: 'Broadcasts · EduVoice',
      },
      {
        path: 'analytics',
        loadComponent: () => import('./features/analytics/analytics.component').then((m) => m.AnalyticsComponent),
        title: 'Analytics · EduVoice',
      },
      {
        path: 'exam-schedule',
        loadComponent: () => import('./features/exam-schedule/exam-schedule.component').then((m) => m.ExamScheduleComponent),
        title: 'Exam Schedule · EduVoice',
      },
      {
        path: 'homework',
        loadComponent: () => import('./features/homework/homework.component').then((m) => m.HomeworkComponent),
        title: 'Homework · EduVoice',
      },
      {
        path: 'dropout-risk',
        loadComponent: () => import('./features/dropout-risk/dropout-risk.component').then((m) => m.DropoutRiskComponent),
        title: 'Dropout Risk · EduVoice',
      },
      {
        path: 'fee-collection',
        loadComponent: () => import('./features/fee-collection/fee-collection.component').then((m) => m.FeeCollectionComponent),
        title: 'Fee Collection · EduVoice',
      },
      {
        path: 'parent-engagement',
        loadComponent: () => import('./features/parent-engagement/parent-engagement.component').then((m) => m.ParentEngagementComponent),
        title: 'Parent Engagement · EduVoice',
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then((m) => m.ReportsComponent),
        title: 'Academic Reports · EduVoice',
      },
      {
        path: 'staff-operations',
        loadComponent: () => import('./features/staff-operations/staff-operations.component').then((m) => m.StaffOperationsComponent),
        title: 'Staff Operations · EduVoice',
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then((m) => m.SettingsComponent),
        title: 'Settings · EduVoice',
      },
      // Teacher portal
      {
        path: 'teacher/dashboard',
        loadComponent: () => import('./features/teacher/teacher-dashboard.component').then((m) => m.TeacherDashboardComponent),
        title: 'Teacher Portal · EduVoice',
      },
      {
        path: 'teacher/attendance',
        loadComponent: () => import('./features/teacher/attendance/attendance.component').then((m) => m.AttendanceComponent),
        title: 'Mark Attendance · EduVoice',
      },
      {
        path: 'teacher/marks',
        loadComponent: () => import('./features/teacher/marks/marks-upload.component').then((m) => m.MarksUploadComponent),
        title: 'Upload Marks · EduVoice',
      },
      {
        path: 'teacher/homework',
        loadComponent: () => import('./features/teacher/homework/teacher-homework.component').then((m) => m.TeacherHomeworkComponent),
        title: 'Assign Homework · EduVoice',
      },
      {
        path: 'admin/schools',
        loadComponent: () => import('./features/admin/admin-schools.component').then((m) => m.AdminSchoolsComponent),
        canActivate: [roleGuard(['SuperAdmin'])],
        title: 'Schools · EduVoice Admin',
      },
    ],
  },
  {
    path: 'portal',
    loadComponent: () => import('./features/portal/parent-portal.component').then((m) => m.ParentPortalComponent),
    title: 'Parent Portal · EduVoice',
  },
  { path: '**', redirectTo: 'dashboard' },
];
