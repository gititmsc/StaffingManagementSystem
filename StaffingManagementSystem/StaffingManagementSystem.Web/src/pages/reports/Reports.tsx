import { useEffect, useState, type KeyboardEvent } from "react";
import { useNavigate } from "react-router-dom";
import { CANDIDATE_STATUS_LABELS, CANDIDATE_STATUS_OPTIONS } from "@/constants/candidates";
import {
  reportsService,
  type CandidateReportSummary,
  type CandidateSearchParams,
  type CandidateSearchResult,
  type NameCount,
} from "@/services/reportsService";
import "./Reports.css";

const EMPTY_FILTERS: CandidateSearchParams = {
  skills: [],
  skillMatchMode: "OR",
  sortBy: "Created",
  sortDescending: true,
  page: 1,
  pageSize: 10,
};

function formatDate(value?: string | null): string {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleDateString();
}

export default function Reports() {
  const navigate = useNavigate();

  const [filters, setFilters] = useState<CandidateSearchParams>(EMPTY_FILTERS);
  const [skillInput, setSkillInput] = useState("");

  const [results, setResults] = useState<CandidateSearchResult | null>(null);
  const [isSearching, setIsSearching] = useState(true);
  const [searchError, setSearchError] = useState<string | null>(null);
  const [isExporting, setIsExporting] = useState(false);
  const [exportError, setExportError] = useState<string | null>(null);

  const [summary, setSummary] = useState<CandidateReportSummary | null>(null);
  const [summaryError, setSummaryError] = useState<string | null>(null);

  const runSearch = async (overrides?: Partial<CandidateSearchParams>) => {
    const nextFilters = { ...filters, ...overrides };
    setFilters(nextFilters);
    setIsSearching(true);
    setSearchError(null);

    const response = await reportsService.search(nextFilters);

    if (!response.success || !response.data) {
      setSearchError(response.message || "Unable to run this search.");
      setIsSearching(false);
      return;
    }

    setResults(response.data);
    setIsSearching(false);
  };

  const loadSummary = async () => {
    const response = await reportsService.getSummary();

    if (!response.success || !response.data) {
      setSummaryError(response.message || "Unable to load report summaries.");
      return;
    }

    setSummary(response.data);
  };

  useEffect(() => {
    runSearch({ page: 1 });
    loadSummary();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const addSkillChip = () => {
    const value = skillInput.trim();
    if (!value) return;
    if ((filters.skills ?? []).some((s) => s.toLowerCase() === value.toLowerCase())) {
      setSkillInput("");
      return;
    }

    setFilters((prev) => ({ ...prev, skills: [...(prev.skills ?? []), value] }));
    setSkillInput("");
  };

  const removeSkillChip = (skill: string) => {
    setFilters((prev) => ({ ...prev, skills: (prev.skills ?? []).filter((s) => s !== skill) }));
  };

  const handleSkillKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter" || event.key === ",") {
      event.preventDefault();
      addSkillChip();
    }
  };

  const handleReset = () => {
    setFilters(EMPTY_FILTERS);
    setSkillInput("");
    runSearch({ ...EMPTY_FILTERS });
  };

  const handleExport = async () => {
    setIsExporting(true);
    setExportError(null);
    try {
      await reportsService.exportCsv(filters);
    } catch {
      setExportError("Unable to export results. Please try again.");
    } finally {
      setIsExporting(false);
    }
  };

  const applySkillFilter = (skillName: string) => {
    const nextFilters: CandidateSearchParams = { ...filters, skills: [skillName], skillMatchMode: "OR", page: 1 };
    setSkillInput("");
    runSearch(nextFilters);
  };

  const applyExperienceBand = (bandLabel: string) => {
    const bands: Record<string, [number | undefined, number | undefined]> = {
      "0-2 yrs": [0, 2],
      "2-5 yrs": [2, 5],
      "5-10 yrs": [5, 10],
      "10+ yrs": [10, undefined],
    };
    const [min, max] = bands[bandLabel] ?? [undefined, undefined];
    runSearch({ minExperience: min, maxExperience: max, page: 1 });
  };

  const applyCompanyFilter = (companyName: string) => {
    runSearch({ company: companyName, page: 1 });
  };

  const goToPage = (page: number) => {
    runSearch({ page });
  };

  const maxCount = (rows: NameCount[]): number => Math.max(1, ...rows.map((r) => r.count));

  return (
    <div className="container py-4">
      <div className="reports-header">
        <h1 className="h4 mb-1" style={{ color: "var(--itm-primary)" }}>
          Search &amp; Reports
        </h1>
        <p className="text-muted mb-0">
          Combine filters to search the candidate pool, or drill into the skill, experience and company reports below.
        </p>
      </div>

      <section className="reports-filter-panel">
        <div className="row g-3">
          <div className="col-12 col-lg-6">
            <label className="reports-label">Skills</label>
            <div className="reports-skill-input">
              {(filters.skills ?? []).map((skill) => (
                <span className="reports-skill-chip" key={skill}>
                  {skill}
                  <button type="button" onClick={() => removeSkillChip(skill)} aria-label={`Remove ${skill}`}>
                    <i className="bi bi-x" aria-hidden="true" />
                  </button>
                </span>
              ))}
              <input
                type="text"
                placeholder="Type a skill and press Enter"
                value={skillInput}
                onChange={(event) => setSkillInput(event.target.value)}
                onKeyDown={handleSkillKeyDown}
                onBlur={addSkillChip}
              />
            </div>
            {(filters.skills ?? []).length > 1 && (
              <div className="reports-skill-mode">
                <label>
                  <input
                    type="radio"
                    checked={filters.skillMatchMode !== "AND"}
                    onChange={() => setFilters((prev) => ({ ...prev, skillMatchMode: "OR" }))}
                  />
                  Match any (OR)
                </label>
                <label>
                  <input
                    type="radio"
                    checked={filters.skillMatchMode === "AND"}
                    onChange={() => setFilters((prev) => ({ ...prev, skillMatchMode: "AND" }))}
                  />
                  Match all (AND)
                </label>
              </div>
            )}
          </div>

          <div className="col-6 col-lg-3">
            <label className="reports-label">Min Experience (yrs)</label>
            <input
              type="number"
              min="0"
              step="0.5"
              className="form-control"
              value={filters.minExperience ?? ""}
              onChange={(event) =>
                setFilters((prev) => ({ ...prev, minExperience: event.target.value ? Number(event.target.value) : undefined }))
              }
            />
          </div>

          <div className="col-6 col-lg-3">
            <label className="reports-label">Max Experience (yrs)</label>
            <input
              type="number"
              min="0"
              step="0.5"
              className="form-control"
              value={filters.maxExperience ?? ""}
              onChange={(event) =>
                setFilters((prev) => ({ ...prev, maxExperience: event.target.value ? Number(event.target.value) : undefined }))
              }
            />
          </div>

          <div className="col-12 col-lg-4">
            <label className="reports-label">Company (current or past)</label>
            <input
              type="text"
              className="form-control"
              value={filters.company ?? ""}
              onChange={(event) => setFilters((prev) => ({ ...prev, company: event.target.value }))}
            />
          </div>

          <div className="col-12 col-lg-4">
            <label className="reports-label">Current Designation</label>
            <input
              type="text"
              className="form-control"
              value={filters.designation ?? ""}
              onChange={(event) => setFilters((prev) => ({ ...prev, designation: event.target.value }))}
            />
          </div>

          <div className="col-12 col-lg-4">
            <label className="reports-label">Location</label>
            <input
              type="text"
              className="form-control"
              value={filters.location ?? ""}
              onChange={(event) => setFilters((prev) => ({ ...prev, location: event.target.value }))}
            />
          </div>

          <div className="col-6 col-lg-3">
            <label className="reports-label">Status</label>
            <select
              className="form-select"
              value={filters.status ?? ""}
              onChange={(event) => setFilters((prev) => ({ ...prev, status: event.target.value || undefined }))}
            >
              <option value="">All statuses</option>
              {CANDIDATE_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="col-6 col-lg-3">
            <label className="reports-label">Sort By</label>
            <select
              className="form-select"
              value={filters.sortBy}
              onChange={(event) =>
                setFilters((prev) => ({ ...prev, sortBy: event.target.value as CandidateSearchParams["sortBy"] }))
              }
            >
              <option value="Created">Date Added</option>
              <option value="Experience">Experience</option>
              <option value="Name">Name</option>
            </select>
          </div>

          <div className="col-6 col-lg-3">
            <label className="reports-label">Direction</label>
            <select
              className="form-select"
              value={filters.sortDescending ? "desc" : "asc"}
              onChange={(event) => setFilters((prev) => ({ ...prev, sortDescending: event.target.value === "desc" }))}
            >
              <option value="desc">Descending</option>
              <option value="asc">Ascending</option>
            </select>
          </div>
        </div>

        <div className="reports-filter-actions">
          <button type="button" className="reports-btn reports-btn--ghost" onClick={handleReset}>
            Reset
          </button>
          <button type="button" className="reports-btn reports-btn--outline" onClick={handleExport} disabled={isExporting}>
            {isExporting && <span className="login-spinner" aria-hidden="true" />}
            <i className="bi bi-download" aria-hidden="true" />
            Export CSV
          </button>
          <button type="button" className="reports-btn reports-btn--primary" onClick={() => runSearch({ page: 1 })}>
            <i className="bi bi-search" aria-hidden="true" />
            Search
          </button>
        </div>

        {exportError && (
          <div className="reports-alert reports-alert--error" role="alert">
            <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
            <span>{exportError}</span>
          </div>
        )}
      </section>

      <section className="reports-results-card">
        {isSearching ? (
          <div className="reports-empty">
            <span
              className="login-spinner"
              style={{ borderTopColor: "var(--itm-primary)", borderColor: "rgba(22,58,99,0.2)" }}
            />
            <span>Searching...</span>
          </div>
        ) : searchError ? (
          <div className="reports-empty reports-empty--error">
            <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
            <span>{searchError}</span>
          </div>
        ) : !results || results.items.length === 0 ? (
          <div className="reports-empty">
            <i className="bi bi-search" aria-hidden="true" />
            <span>No candidates match these filters.</span>
          </div>
        ) : (
          <>
            <div className="reports-results-summary">
              {results.totalCount} candidate{results.totalCount === 1 ? "" : "s"} found
            </div>
            <table className="table candidates-table align-middle mb-0">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Location</th>
                  <th>Experience</th>
                  <th>Current Company</th>
                  <th>Skills</th>
                  <th>Status</th>
                  <th>Added</th>
                </tr>
              </thead>
              <tbody>
                {results.items.map((row) => (
                  <tr key={row.id}>
                    <td>
                      <button type="button" className="candidates-name-link" onClick={() => navigate(`/candidates/${row.id}`)}>
                        {row.fullName}
                      </button>
                    </td>
                    <td>{row.currentLocation || "—"}</td>
                    <td>{row.totalExperienceYears} yrs</td>
                    <td>{row.currentCompany || "—"}</td>
                    <td>
                      <div className="candidates-skill-chips">
                        {row.skills.slice(0, 3).map((skill) => (
                          <span key={skill} className="candidates-skill-chip">
                            {skill}
                          </span>
                        ))}
                        {row.skills.length > 3 && <span className="candidates-skill-chip">+{row.skills.length - 3}</span>}
                      </div>
                    </td>
                    <td>{CANDIDATE_STATUS_LABELS[row.status] ?? row.status}</td>
                    <td>{formatDate(row.createdAtUtc)}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            {results.totalPages > 1 && (
              <div className="reports-pagination">
                <button
                  type="button"
                  className="reports-page-btn"
                  disabled={results.page <= 1}
                  onClick={() => goToPage(results.page - 1)}
                >
                  <i className="bi bi-chevron-left" aria-hidden="true" />
                  Previous
                </button>
                <span className="reports-page-indicator">
                  Page {results.page} of {results.totalPages}
                </span>
                <button
                  type="button"
                  className="reports-page-btn"
                  disabled={results.page >= results.totalPages}
                  onClick={() => goToPage(results.page + 1)}
                >
                  Next
                  <i className="bi bi-chevron-right" aria-hidden="true" />
                </button>
              </div>
            )}
          </>
        )}
      </section>

      <div className="reports-summary-grid">
        <section className="reports-summary-card">
          <h2 className="reports-summary-card__title">Skill-wise</h2>
          {summaryError ? (
            <p className="reports-empty--error mb-0">{summaryError}</p>
          ) : !summary || summary.skillCounts.length === 0 ? (
            <p className="candidates-empty mb-0">No skill data yet.</p>
          ) : (
            <ul className="reports-bar-list">
              {summary.skillCounts.map((row) => (
                <li key={row.name}>
                  <button type="button" onClick={() => applySkillFilter(row.name)}>
                    <span className="reports-bar-list__label">{row.name}</span>
                    <span className="reports-bar-list__track">
                      <span
                        className="reports-bar-list__fill"
                        style={{ width: `${(row.count / maxCount(summary.skillCounts)) * 100}%` }}
                      />
                    </span>
                    <span className="reports-bar-list__count">{row.count}</span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="reports-summary-card">
          <h2 className="reports-summary-card__title">Experience-wise</h2>
          {!summary ? (
            <p className="candidates-empty mb-0">No experience data yet.</p>
          ) : (
            <ul className="reports-bar-list">
              {summary.experienceBandCounts.map((row) => (
                <li key={row.name}>
                  <button type="button" onClick={() => applyExperienceBand(row.name)}>
                    <span className="reports-bar-list__label">{row.name}</span>
                    <span className="reports-bar-list__track">
                      <span
                        className="reports-bar-list__fill"
                        style={{ width: `${(row.count / maxCount(summary.experienceBandCounts)) * 100}%` }}
                      />
                    </span>
                    <span className="reports-bar-list__count">{row.count}</span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="reports-summary-card">
          <h2 className="reports-summary-card__title">Company-wise</h2>
          {!summary || summary.companyCounts.length === 0 ? (
            <p className="candidates-empty mb-0">No company data yet.</p>
          ) : (
            <ul className="reports-bar-list">
              {summary.companyCounts.map((row) => (
                <li key={row.name}>
                  <button type="button" onClick={() => applyCompanyFilter(row.name)}>
                    <span className="reports-bar-list__label">{row.name}</span>
                    <span className="reports-bar-list__track">
                      <span
                        className="reports-bar-list__fill"
                        style={{ width: `${(row.count / maxCount(summary.companyCounts)) * 100}%` }}
                      />
                    </span>
                    <span className="reports-bar-list__count">{row.count}</span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </div>
  );
}
