import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer class="footer">
      <div class="footer-container">
        <div class="footer-col">
          <h4 class="footer-heading">CloudSaga E-Commerce</h4>
          <p class="footer-desc">
            Production-grade event-driven microservices platform implementing Choreographed & Orchestrated Saga patterns with .NET 9 and Angular 19.
          </p>
        </div>
        <div class="footer-col">
          <h5 class="tech-title">Architecture & Tech Stack</h5>
          <div class="badges">
            <span class="badge">.NET 9 Web API</span>
            <span class="badge">Angular 19 Signals</span>
            <span class="badge">Saga Pattern</span>
            <span class="badge">Azure Service Bus</span>
            <span class="badge">Transactional Outbox</span>
            <span class="badge">Inbox Deduplication</span>
            <span class="badge">Ocelot Gateway</span>
            <span class="badge">SQL Server</span>
          </div>
        </div>
      </div>
      <div class="footer-bottom">
        <p>© 2026 CloudSaga Microservices Platform. All rights reserved.</p>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      background: #0f172a;
      color: #94a3b8;
      border-top: 1px solid #1e293b;
      padding: 2.5rem 1.5rem 1.5rem;
      margin-top: auto;
    }
    .footer-container {
      max-width: 1280px;
      margin: 0 auto;
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 2rem;
    }
    .footer-heading {
      color: #f8fafc;
      font-size: 1.1rem;
      margin-bottom: 0.5rem;
    }
    .footer-desc {
      font-size: 0.85rem;
      line-height: 1.5;
      color: #64748b;
    }
    .tech-title {
      color: #cbd5e1;
      font-size: 0.9rem;
      margin-bottom: 0.75rem;
    }
    .badges {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .badge {
      background: #1e293b;
      border: 1px solid #334155;
      color: #60a5fa;
      font-size: 0.75rem;
      font-weight: 500;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
    }
    .footer-bottom {
      max-width: 1280px;
      margin: 2rem auto 0;
      padding-top: 1rem;
      border-top: 1px solid #1e293b;
      text-align: center;
      font-size: 0.75rem;
      color: #475569;
    }
    @media (max-width: 768px) {
      .footer-container {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class FooterComponent {}
