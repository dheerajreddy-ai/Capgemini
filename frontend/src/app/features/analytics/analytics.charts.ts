import { ApexOptions } from 'ng-apexcharts';
import { DashboardStats } from '../../core/models/models';

const weekdays = (week: DashboardStats['callsThisWeek']) =>
  week.map((d) => new Date(d.date).toLocaleDateString('en-IN', { weekday: 'short' }));

export function barChart(week: DashboardStats['callsThisWeek']): ApexOptions {
  return {
    series: [{ name: 'Calls', data: week.map((d) => d.completed + d.noAnswer) }],
    chart: { type: 'bar', height: 320, fontFamily: 'Inter, sans-serif', toolbar: { show: false } },
    colors: ['#1E40AF'],
    plotOptions: { bar: { borderRadius: 8, columnWidth: '45%', distributed: false } },
    dataLabels: { enabled: false },
    xaxis: { categories: weekdays(week), labels: { style: { colors: '#94A3B8' } }, axisBorder: { show: false }, axisTicks: { show: false } },
    yaxis: { labels: { style: { colors: '#94A3B8' } } },
    grid: { borderColor: '#EEF2F8', strokeDashArray: 4 },
    legend: { show: false },
    fill: { type: 'gradient', gradient: { gradientToColors: ['#3B82F6'], shadeIntensity: 1, opacityFrom: 1, opacityTo: 0.85 } },
  };
}

export function typePie(s?: DashboardStats['sentimentBreakdown']): ApexOptions {
  const v = s ?? { positive: 0, neutral: 0, negative: 0 };
  return {
    series: [v.positive, v.neutral, v.negative],
    chart: { type: 'donut', height: 320, fontFamily: 'Inter, sans-serif' },
    labels: ['Positive', 'Neutral', 'Negative'],
    colors: ['#10B981', '#94A3B8', '#EF4444'],
    stroke: { width: 0 },
    dataLabels: { enabled: true },
    legend: { position: 'bottom', fontWeight: 600 },
    plotOptions: { pie: { donut: { size: '68%' } } },
  };
}

export function sentimentTrend(week: DashboardStats['callsThisWeek']): ApexOptions {
  return {
    series: [{ name: 'Completed', data: week.map((d) => d.completed) }],
    chart: { type: 'line', height: 300, fontFamily: 'Inter, sans-serif', toolbar: { show: false } },
    colors: ['#6366F1'],
    stroke: { curve: 'smooth', width: 4 },
    dataLabels: { enabled: false },
    xaxis: { categories: weekdays(week), labels: { style: { colors: '#94A3B8' } }, axisBorder: { show: false }, axisTicks: { show: false } },
    yaxis: { labels: { style: { colors: '#94A3B8' } } },
    grid: { borderColor: '#EEF2F8', strokeDashArray: 4 },
    legend: { show: false },
    markers: { size: 5, colors: ['#6366F1'], strokeColors: '#fff', strokeWidth: 2 },
  };
}
