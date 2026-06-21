import { ApexOptions } from 'ng-apexcharts';
import { DashboardStats } from '../../core/models/models';

const PRIMARY = '#1E40AF';
const PRIMARY_LIGHT = '#3B82F6';
const NEUTRAL = '#94A3B8';

export function areaChart(week: DashboardStats['callsThisWeek']): ApexOptions {
  const labels = week.map((d) =>
    new Date(d.date).toLocaleDateString('en-IN', { weekday: 'short' }),
  );
  return {
    series: [
      { name: 'Completed', data: week.map((d) => d.completed) },
      { name: 'No Answer', data: week.map((d) => d.noAnswer) },
    ],
    chart: {
      type: 'area',
      height: 300,
      fontFamily: 'Inter, sans-serif',
      toolbar: { show: false },
      zoom: { enabled: false },
      animations: { enabled: true, speed: 600 },
    },
    colors: [PRIMARY_LIGHT, NEUTRAL],
    dataLabels: { enabled: false },
    stroke: { curve: 'smooth', width: [3, 2] },
    fill: {
      type: 'gradient',
      gradient: { shadeIntensity: 1, opacityFrom: 0.35, opacityTo: 0.02, stops: [0, 90, 100] },
    },
    grid: { borderColor: '#EEF2F8', strokeDashArray: 4, padding: { left: 8, right: 8 } },
    xaxis: {
      categories: labels,
      labels: { style: { colors: '#94A3B8', fontSize: '12px' } },
      axisBorder: { show: false },
      axisTicks: { show: false },
    },
    yaxis: { labels: { style: { colors: '#94A3B8', fontSize: '12px' } } },
    legend: { position: 'top', horizontalAlign: 'right', markers: { size: 6 }, fontWeight: 600 },
    tooltip: { theme: 'light' },
  };
}

export function donutChart(outcome?: DashboardStats['outcomeBreakdown']): ApexOptions {
  const o = outcome ?? { feesConfirmed: 0, complaintFiled: 0, noAnswer: 0, callbackRequested: 0 };
  return {
    series: [o.feesConfirmed, o.complaintFiled, o.noAnswer, o.callbackRequested],
    chart: { type: 'donut', height: 300, fontFamily: 'Inter, sans-serif' },
    labels: ['Fees Confirmed', 'Complaint Filed', 'No Answer', 'Callback Requested'],
    colors: ['#10B981', '#EF4444', '#94A3B8', '#6366F1'],
    stroke: { width: 0 },
    dataLabels: { enabled: false },
    legend: { position: 'bottom', fontWeight: 600, markers: { size: 6 }, itemMargin: { horizontal: 8, vertical: 4 } },
    plotOptions: {
      pie: {
        donut: {
          size: '72%',
          labels: {
            show: true,
            total: {
              show: true,
              label: 'Total Calls',
              fontSize: '13px',
              color: '#94A3B8',
              fontWeight: 600,
              formatter: (w) => `${w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0)}`,
            },
            value: { fontSize: '26px', fontWeight: 800, color: '#0F172A' },
          },
        },
      },
    },
  };
}
