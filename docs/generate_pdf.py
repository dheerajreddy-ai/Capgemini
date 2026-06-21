#!/usr/bin/env python3
"""Generate the EduVoice product documentation PDF with diagrams."""

from reportlab.lib.pagesizes import A4
from reportlab.lib.units import mm
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_JUSTIFY
from reportlab.platypus import (
    BaseDocTemplate, PageTemplate, Frame, Paragraph, Spacer, Table, TableStyle,
    Flowable, PageBreak, KeepTogether, NextPageTemplate,
)
from reportlab.pdfgen import canvas as canvasmod

# ---------------------------------------------------------------- palette
INK      = colors.HexColor("#0F172A")
PRIMARY  = colors.HexColor("#1E40AF")
PRIMARY2 = colors.HexColor("#3B82F6")
VIOLET   = colors.HexColor("#6D28D9")
GREEN    = colors.HexColor("#059669")
AMBER    = colors.HexColor("#D97706")
RED      = colors.HexColor("#DC2626")
SLATE    = colors.HexColor("#64748B")
LIGHT    = colors.HexColor("#F1F5F9")
CARD     = colors.HexColor("#F8FAFC")
BORDER   = colors.HexColor("#E2E8F0")
WHITE    = colors.white

PAGE_W, PAGE_H = A4

# ---------------------------------------------------------------- styles
styles = getSampleStyleSheet()

def S(name, **kw):
    styles.add(ParagraphStyle(name, **kw))

S("H1", fontName="Helvetica-Bold", fontSize=22, textColor=INK, leading=26, spaceAfter=6)
S("H2", fontName="Helvetica-Bold", fontSize=15, textColor=PRIMARY, leading=19, spaceBefore=14, spaceAfter=6)
S("H3", fontName="Helvetica-Bold", fontSize=11.5, textColor=INK, leading=15, spaceBefore=8, spaceAfter=3)
S("Body", fontName="Helvetica", fontSize=10, textColor=colors.HexColor("#1E293B"), leading=15, alignment=TA_JUSTIFY, spaceAfter=6)
S("Small", fontName="Helvetica", fontSize=8.5, textColor=SLATE, leading=12)
S("Lead", fontName="Helvetica", fontSize=11, textColor=colors.HexColor("#334155"), leading=16, spaceAfter=6)
S("EvBullet", fontName="Helvetica", fontSize=9.7, textColor=colors.HexColor("#1E293B"), leading=14, leftIndent=14, bulletIndent=2, spaceAfter=4)
S("CardTitle", fontName="Helvetica-Bold", fontSize=10.5, textColor=INK, leading=13)
S("CardBody", fontName="Helvetica", fontSize=8.8, textColor=SLATE, leading=12)
S("KStat", fontName="Helvetica-Bold", fontSize=15, textColor=PRIMARY, leading=19, alignment=TA_CENTER)
S("KLabel", fontName="Helvetica", fontSize=8, textColor=SLATE, leading=10, alignment=TA_CENTER)
S("WhiteH", fontName="Helvetica-Bold", fontSize=34, textColor=WHITE, leading=38)
S("WhiteSub", fontName="Helvetica", fontSize=13, textColor=colors.HexColor("#DBEAFE"), leading=18)
S("WhiteSmall", fontName="Helvetica", fontSize=9.5, textColor=colors.HexColor("#BFDBFE"), leading=13)


# ================================================================ diagrams
class PipelineDiagram(Flowable):
    """The end-to-end call pipeline, drawn as connected boxes with arrows."""
    def __init__(self, width=460, height=330):
        super().__init__()
        self.width = width
        self.height = height

    def _box(self, c, x, y, w, h, title, sub, fill, tcol=WHITE):
        c.setFillColor(fill)
        c.roundRect(x, y, w, h, 7, fill=1, stroke=0)
        c.setFillColor(tcol)
        c.setFont("Helvetica-Bold", 9)
        c.drawCentredString(x + w / 2, y + h - 15, title)
        c.setFont("Helvetica", 6.7)
        c.setFillColor(colors.HexColor("#E0E7FF") if tcol == WHITE else SLATE)
        # wrap sub into lines
        words, line, lines = sub.split(), "", []
        for wd in words:
            if c.stringWidth(line + " " + wd, "Helvetica", 6.7) < w - 10:
                line = (line + " " + wd).strip()
            else:
                lines.append(line); line = wd
        lines.append(line)
        for i, ln in enumerate(lines[:3]):
            c.drawCentredString(x + w / 2, y + h - 26 - i * 8, ln)

    def _arrow(self, c, x1, y1, x2, y2):
        c.setStrokeColor(SLATE)
        c.setLineWidth(1.4)
        c.line(x1, y1, x2, y2)
        # arrowhead
        import math
        ang = math.atan2(y2 - y1, x2 - x1)
        c.setFillColor(SLATE)
        s = 5
        c.setLineWidth(0)
        p = c.beginPath()
        p.moveTo(x2, y2)
        p.lineTo(x2 - s * math.cos(ang - 0.4), y2 - s * math.sin(ang - 0.4))
        p.lineTo(x2 - s * math.cos(ang + 0.4), y2 - s * math.sin(ang + 0.4))
        p.close()
        c.drawPath(p, fill=1, stroke=0)

    def draw(self):
        c = self.canv
        bw, bh = 118, 46
        col1, col2, col3 = 8, 171, 334

        # Row 1
        r1y = 270
        self._box(c, col1, r1y, bw, bh, "Dashboard", "School admin picks students & launches campaign", PRIMARY)
        self._box(c, col2, r1y, bw, bh, "Backend API", ".NET 9 · checks calling window, DND, duplicates", VIOLET)
        self._box(c, col3, r1y, bw, bh, "Vapi.ai", "Places the outbound phone call", colors.HexColor("#0EA5E9"))

        # Row 2
        r2y = 188
        self._box(c, col3, r2y, bw, bh, "ElevenLabs", "Speaks in Telugu (Telangana / Andhra voice)", GREEN)
        self._box(c, col2, r2y, bw, bh, "Parent's Phone", "Hears AI agent, talks back in Telugu+English", AMBER)
        self._box(c, col1, r2y, bw, bh, "Deepgram", "Understands the parent's reply (speech-to-text)", colors.HexColor("#7C3AED"))

        # Row 3
        r3y = 106
        self._box(c, col1, r3y, bw, bh, "Claude AI", "Reads transcript: sentiment, fees, complaint", PRIMARY2)
        self._box(c, col2, r3y, bw, bh, "Database", "Saves call, transcript, recording, outcome", INK)
        self._box(c, col3, r3y, bw, bh, "Actions", "Auto WhatsApp UPI link, complaint, retry", GREEN)

        # Row 4
        r4y = 28
        self._box(c, col2, r4y, bw, bh, "Parent WhatsApp", "Payment link + call summary delivered", colors.HexColor("#16A34A"))

        # arrows
        self._arrow(c, col1 + bw, r1y + bh/2, col2, r1y + bh/2)          # dash -> api
        self._arrow(c, col2 + bw, r1y + bh/2, col3, r1y + bh/2)          # api -> vapi
        self._arrow(c, col3 + bw/2, r1y, col3 + bw/2, r2y + bh)         # vapi -> eleven
        self._arrow(c, col3, r2y + bh/2, col2 + bw, r2y + bh/2)         # eleven -> parent
        self._arrow(c, col2, r2y + bh/2, col1 + bw, r2y + bh/2)         # parent -> deepgram
        self._arrow(c, col1 + bw/2, r2y, col1 + bw/2, r3y + bh)         # deepgram -> claude
        self._arrow(c, col1 + bw, r3y + bh/2, col2, r3y + bh/2)         # claude -> db
        self._arrow(c, col2 + bw, r3y + bh/2, col3, r3y + bh/2)         # db -> actions
        self._arrow(c, col3 + bw/2, r3y, col2 + bw, r4y + bh/2)         # actions -> whatsapp


class TenantDiagram(Flowable):
    """Multi-tenant isolation diagram."""
    def __init__(self, width=460, height=170):
        super().__init__()
        self.width = width
        self.height = height

    def draw(self):
        c = self.canv
        # platform box
        c.setFillColor(LIGHT)
        c.roundRect(0, 0, self.width, self.height, 10, fill=1, stroke=0)
        c.setFillColor(PRIMARY)
        c.setFont("Helvetica-Bold", 11)
        c.drawString(16, self.height - 24, "EduVoice Platform  (one codebase, one database)")

        schools = [
            ("Sri Vidya School", "srvk.eduvoice.in", PRIMARY),
            ("Bhashyam School", "bhashyam.eduvoice.in", VIOLET),
            ("Narayana School", "narayana.eduvoice.in", GREEN),
        ]
        bw, gap = 138, 14
        startx = 16
        y = 26
        for i, (name, dom, col) in enumerate(schools):
            x = startx + i * (bw + gap)
            c.setFillColor(WHITE)
            c.roundRect(x, y, bw, 96, 8, fill=1, stroke=1)
            c.setStrokeColor(BORDER)
            c.setFillColor(col)
            c.roundRect(x, y + 78, bw, 18, 8, fill=1, stroke=0)
            c.setFillColor(WHITE)
            c.setFont("Helvetica-Bold", 8.5)
            c.drawCentredString(x + bw/2, y + 83, name)
            c.setFillColor(SLATE)
            c.setFont("Helvetica", 7)
            c.drawCentredString(x + bw/2, y + 64, dom)
            for j, item in enumerate(["Own logo & colours", "Own phone number", "Own students & data", "Isolated — never mixed"]):
                c.setFillColor(col)
                c.circle(x + 12, y + 50 - j*13, 1.6, fill=1, stroke=0)
                c.setFillColor(INK)
                c.setFont("Helvetica", 7)
                c.drawString(x + 18, y + 47 - j*13, item)


def hr(width=460):
    t = Table([[""]], colWidths=[width], rowHeights=[1])
    t.setStyle(TableStyle([("LINEBELOW", (0,0), (-1,-1), 0.7, BORDER)]))
    return t


def stat_row(items):
    """items: list of (number, label)"""
    cells, sty = [], [
        ("VALIGN", (0,0), (-1,-1), "MIDDLE"),
        ("ROWBACKGROUNDS", (0,0), (-1,-1), [CARD]),
        ("BOX", (0,0), (-1,-1), 0.7, BORDER),
        ("INNERGRID", (0,0), (-1,-1), 0.7, BORDER),
        ("TOPPADDING", (0,0), (-1,-1), 10),
        ("BOTTOMPADDING", (0,0), (-1,-1), 10),
    ]
    row = []
    for num, lab in items:
        row.append(Paragraph(f"{num}<br/><font size=8 color='#64748B'>{lab}</font>",
                             ParagraphStyle("x", parent=styles["KStat"])))
    t = Table([row], colWidths=[460/len(items)]*len(items))
    t.setStyle(TableStyle(sty))
    return t


def feature_card(num, title, body, col):
    inner = Table(
        [[Paragraph(f"<font color='white'><b>{num}</b></font>", styles["CardTitle"])],
         [Paragraph(f"<b>{title}</b>", styles["CardTitle"])],
         [Paragraph(body, styles["CardBody"])]],
        colWidths=[208],
    )
    inner.setStyle(TableStyle([
        ("BACKGROUND", (0,0), (0,0), col),
        ("TOPPADDING", (0,0), (0,0), 5), ("BOTTOMPADDING", (0,0), (0,0), 5),
        ("LEFTPADDING", (0,0), (0,0), 10), ("RIGHTPADDING", (0,0), (0,0), 10),
        ("TOPPADDING", (0,1), (-1,-1), 4), ("LEFTPADDING", (0,1), (-1,-1), 10),
        ("RIGHTPADDING", (0,1), (-1,-1), 10),
        ("BACKGROUND", (0,1), (-1,-1), WHITE),
        ("BOX", (0,0), (-1,-1), 0.7, BORDER),
    ]))
    return inner


# ================================================================ page deco
def cover_bg(c, doc):
    c.saveState()
    c.setFillColor(PRIMARY)
    c.rect(0, 0, PAGE_W, PAGE_H, fill=1, stroke=0)
    # diagonal accent
    c.setFillColor(colors.HexColor("#1B3AA0"))
    p = c.beginPath(); p.moveTo(0, PAGE_H); p.lineTo(PAGE_W, PAGE_H); p.lineTo(PAGE_W, PAGE_H-230); p.lineTo(0, PAGE_H-140); p.close()
    c.drawPath(p, fill=1, stroke=0)
    c.setFillColor(VIOLET)
    c.setFillAlpha(0.35)
    c.circle(PAGE_W-60, 120, 150, fill=1, stroke=0)
    c.setFillAlpha(1)
    c.restoreState()


def content_bg(c, doc):
    c.saveState()
    c.setFillColor(WHITE); c.rect(0, 0, PAGE_W, PAGE_H, fill=1, stroke=0)
    # header band
    c.setFillColor(LIGHT); c.rect(0, PAGE_H-34, PAGE_W, 34, fill=1, stroke=0)
    c.setFillColor(PRIMARY); c.setFont("Helvetica-Bold", 9)
    c.drawString(18*mm, PAGE_H-22, "EduVoice")
    c.setFillColor(SLATE); c.setFont("Helvetica", 8)
    c.drawRightString(PAGE_W-18*mm, PAGE_H-22, "AI Voice Agent for Telugu Schools")
    # footer
    c.setStrokeColor(BORDER); c.setLineWidth(0.7)
    c.line(18*mm, 16*mm, PAGE_W-18*mm, 16*mm)
    c.setFillColor(SLATE); c.setFont("Helvetica", 7.5)
    c.drawString(18*mm, 11*mm, "VCD AI Labs  ·  Confidential")
    c.drawRightString(PAGE_W-18*mm, 11*mm, f"Page {doc.page-1}")
    c.restoreState()


# ================================================================ build
def build():
    doc = BaseDocTemplate(
        "/home/user/Capgemini/docs/EduVoice-Overview.pdf",
        pagesize=A4, leftMargin=18*mm, rightMargin=18*mm,
        topMargin=20*mm, bottomMargin=20*mm,
        title="EduVoice — Product Overview", author="VCD AI Labs",
    )
    fw = PAGE_W - 36*mm
    cover_frame = Frame(0, 0, PAGE_W, PAGE_H, leftPadding=22*mm, rightPadding=22*mm,
                        topPadding=60*mm, bottomPadding=30*mm, id="cover")
    body_frame = Frame(18*mm, 18*mm, fw, PAGE_H-42*mm, id="body")
    doc.addPageTemplates([
        PageTemplate(id="Cover", frames=[cover_frame], onPage=cover_bg),
        PageTemplate(id="Body", frames=[body_frame], onPage=content_bg),
    ])

    E = []

    # ---------- COVER
    E += [
        Spacer(1, 60),
        Paragraph("EduVoice", styles["WhiteH"]),
        Spacer(1, 6),
        Paragraph("An AI voice agent that calls parents in Telugu — automatically.", styles["WhiteSub"]),
        Spacer(1, 26),
        Paragraph("Fee reminders &nbsp;·&nbsp; Progress updates &nbsp;·&nbsp; Complaint collection", styles["WhiteSmall"]),
        Spacer(1, 150),
        Paragraph("Product Overview &amp; How It Works", styles["WhiteSub"]),
        Paragraph("Prepared by VCD AI Labs", styles["WhiteSmall"]),
        NextPageTemplate("Body"),
        PageBreak(),
    ]

    # ---------- PAGE 1: What is it
    E += [
        Paragraph("What is EduVoice?", styles["H1"]),
        Paragraph(
            "EduVoice is a cloud software platform that lets a private school <b>automatically phone every "
            "parent in Telugu</b> — without a human dialling a single number. A school admin uploads their "
            "students from Excel, clicks <i>Launch Campaign</i>, and the system calls hundreds of parents one "
            "by one. The AI speaks naturally in Telugu, listens to the parent's reply, understands it, and "
            "records the outcome on a live dashboard.", styles["Lead"]),
        Paragraph(
            "It is built for private schools across <b>Andhra Pradesh and Telangana</b>. Each school is a "
            "separate <b>tenant</b> — its own web address, logo, colours, phone number and data, fully isolated "
            "from every other school on the platform.", styles["Body"]),
        Spacer(1, 6),
        stat_row([("3", "Call types"), ("11AM–6PM", "Legal calling window"), ("Telugu+Urdu", "Languages"), ("Multi-tenant", "Architecture")]),
        Spacer(1, 12),
        Paragraph("The three jobs it does", styles["H2"]),
        Table([[
            feature_card("01", "Fee Reminders", "Calls parents about pending fees, then auto-sends a UPI payment link on WhatsApp so they can pay instantly.", PRIMARY),
            feature_card("02", "Progress Updates", "Reads out the student's marks, grade and attendance to the parent in friendly Telugu.", VIOLET),
        ], [
            feature_card("03", "Complaint Collection", "Listens to parent complaints during the call, categorises them, and files them on a kanban board.", GREEN),
            feature_card("04", "Attendance Alerts", "Automatically rings parents of students whose attendance has dropped below the school's threshold.", AMBER),
        ]], colWidths=[fw/2, fw/2], hAlign="LEFT", spaceBefore=4),
        PageBreak(),
    ]

    # ---------- PAGE 2: Pipeline diagram
    E += [
        Paragraph("How a single call works", styles["H1"]),
        Paragraph(
            "When a campaign runs, every parent call flows through this pipeline. Each step is a specialised "
            "service; EduVoice orchestrates them and saves the result.", styles["Body"]),
        Spacer(1, 8),
        PipelineDiagram(width=fw),
        Spacer(1, 6),
        hr(fw),
        Spacer(1, 6),
        Paragraph(
            "<b>In plain words:</b> the dashboard tells the backend who to call. The backend checks it is legal "
            "to call (right time, not on Do-Not-Call, not already called today), then asks Vapi to dial. "
            "ElevenLabs gives the AI a Telugu voice; Deepgram turns the parent's speech into text; Claude reads "
            "that text to decide what happened — did they agree to pay, raise a complaint, or ask to be left "
            "alone? Finally the system acts: it WhatsApps a payment link, files a complaint, or schedules a "
            "retry — and the parent gets a tidy summary on WhatsApp.", styles["Body"]),
        PageBreak(),
    ]

    # ---------- PAGE 3: Multi-tenant + safety
    E += [
        Paragraph("One platform, many schools", styles["H1"]),
        Paragraph(
            "EduVoice is <b>multi-tenant</b>. The same software serves every school, but each school only ever "
            "sees its own data. A query for Sri Vidya School can never return Bhashyam School's students — "
            "isolation is enforced on every single database call.", styles["Body"]),
        Spacer(1, 8),
        TenantDiagram(width=fw),
        Spacer(1, 14),
        Paragraph("Built to stay on the right side of the law", styles["H2"]),
        Paragraph(
            "India's telecom regulator (TRAI) has strict rules for automated calls. EduVoice was designed around "
            "them so a school can never accidentally break the law:", styles["Body"]),
    ]
    safety = [
        ("Calling window", "Calls only run 11 AM – 6 PM IST. A campaign scheduled outside this window is rejected; one that is mid-run auto-pauses at 6 PM and resumes next morning."),
        ("AI disclosure", "Every call opens by telling the parent it is an automated AI call and that the call is being recorded — within the first 15 seconds."),
        ("Opt-out", "The parent can press 9 (or just say 'stop calling') to be added to a Do-Not-Call list. Future campaigns skip them automatically."),
        ("DND scrub", "Before each batch, numbers are checked against the national Do-Not-Disturb registry."),
    ]
    for t, b in safety:
        E.append(Paragraph(f"<b>{t}.</b> {b}", styles["EvBullet"], bulletText="•"))
    E += [PageBreak()]

    # ---------- PAGE 4: The 6 capability areas
    E += [
        Paragraph("What makes it production-ready", styles["H1"]),
        Paragraph(
            "Beyond placing calls, EduVoice handles all the messy realities of calling thousands of parents on "
            "Indian mobile networks. These were built in six phases:", styles["Body"]),
        Spacer(1, 6),
    ]
    phases = [
        ("Phase 1", "Legal & Compliance", "Calling-window enforcement, AI disclosure, recording consent, press-9 opt-out, Do-Not-Call list, fee-extension flag.", PRIMARY),
        ("Phase 2", "Call Reliability", "Voicemail detection + voicemail drop, automatic retries (max 2, 2-hour gap), duplicate guard, wave-based dialling (30 calls per wave) to avoid carrier spam blocks.", VIOLET),
        ("Phase 3", "Telugu Language Quality", "Separate Telangana vs Andhra dialect voices, 40+ boosted domain words, mid-sentence Telugu+English code-switching, escalation to human staff when the parent is angry or unclear.", GREEN),
        ("Phase 4", "Payment Intelligence", "UPI payment links sent on WhatsApp, fee-extension handling, dispute flag that pauses reminders, partial-payment tracking.", AMBER),
        ("Phase 5", "Parent Self-Service", "A mobile parent portal: log in with phone + OTP to see fees, marks, attendance and recent calls — and pay by UPI.", colors.HexColor("#0EA5E9")),
        ("Phase 6", "Robustness & Operations", "TRAI DND scrubbing, carrier-health monitoring per phone number, Urdu support for Urdu-medium sections, attendance-alert campaigns, weak-network handling.", RED),
    ]
    rows = []
    for tag, title, body, col in phases:
        rows.append([
            Paragraph(f"<font color='white'><b>{tag}</b></font>", styles["CardBody"]),
            Paragraph(f"<b>{title}</b><br/><font size=8.5 color='#475569'>{body}</font>", styles["CardBody"]),
        ])
    t = Table(rows, colWidths=[58, fw-58])
    tstyle = [
        ("VALIGN", (0,0), (-1,-1), "TOP"),
        ("BOX", (0,0), (-1,-1), 0.7, BORDER),
        ("INNERGRID", (0,0), (-1,-1), 0.7, BORDER),
        ("TOPPADDING", (0,0), (-1,-1), 8), ("BOTTOMPADDING", (0,0), (-1,-1), 8),
        ("LEFTPADDING", (0,0), (-1,-1), 8), ("RIGHTPADDING", (0,0), (-1,-1), 8),
        ("BACKGROUND", (1,0), (1,-1), WHITE),
    ]
    for i, (_, _, _, col) in enumerate(phases):
        tstyle.append(("BACKGROUND", (0,i), (0,i), col))
    t.setStyle(TableStyle(tstyle))
    E.append(t)
    tech = Table([
        [Paragraph("<b>Backend</b>", styles["CardBody"]), Paragraph(".NET 9 · Clean Architecture (Domain / Application / Infrastructure / API) · PostgreSQL · Entity Framework Core", styles["CardBody"])],
        [Paragraph("<b>Frontend</b>", styles["CardBody"]), Paragraph("Angular 22 · Bootstrap 5 · premium white-label design system · ApexCharts dashboards", styles["CardBody"])],
        [Paragraph("<b>Voice &amp; AI</b>", styles["CardBody"]), Paragraph("Vapi.ai (call orchestration) · ElevenLabs (Telugu voice) · Deepgram (speech-to-text) · Claude (understanding)", styles["CardBody"])],
        [Paragraph("<b>Messaging</b>", styles["CardBody"]), Paragraph("Twilio (phone numbers + WhatsApp) · SendGrid (email) · n8n (automation workflows)", styles["CardBody"])],
    ], colWidths=[70, fw-70])
    tech.setStyle(TableStyle([
        ("VALIGN", (0,0), (-1,-1), "TOP"),
        ("BOX", (0,0), (-1,-1), 0.7, BORDER),
        ("INNERGRID", (0,0), (-1,-1), 0.7, BORDER),
        ("ROWBACKGROUNDS", (0,0), (-1,-1), [CARD, WHITE]),
        ("TOPPADDING", (0,0), (-1,-1), 7), ("BOTTOMPADDING", (0,0), (-1,-1), 7),
        ("LEFTPADDING", (0,0), (-1,-1), 9), ("RIGHTPADDING", (0,0), (-1,-1), 9),
    ]))
    E += [
        Spacer(1, 14),
        Paragraph("The technology under the hood", styles["H2"]),
        tech,
    ]

    doc.build(E)
    print("PDF written to docs/EduVoice-Overview.pdf")


if __name__ == "__main__":
    build()
