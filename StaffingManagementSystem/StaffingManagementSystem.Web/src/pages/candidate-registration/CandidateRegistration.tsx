import { useState } from "react";
import { useFieldArray, useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import ReCAPTCHA from "react-google-recaptcha";
import { ITMLogo } from "@/components/brand/ITMLogo";
import { EMPLOYMENT_TYPE_OPTIONS } from "@/constants/candidates";
import { candidateRegistrationService } from "@/services/candidateRegistrationService";
import "@/pages/candidates/CandidateForm.css";
import "./CandidateRegistration.css";

interface FormValues {
  fullName: string;
  email: string;
  phone: string;
  currentLocation: string;
  linkedInUrl: string;
  skills: string;
  experience: {
    companyName: string;
    jobTitle: string;
    employmentType: string;
    startDate: string;
    endDate: string;
    isCurrent: boolean;
    location: string;
    description: string;
  }[];
  education: {
    degree: string;
    institution: string;
    fieldOfStudy: string;
    startYear: string;
    endYear: string;
    isExpected: boolean;
    grade: string;
  }[];
  projects: {
    projectName: string;
    role: string;
    durationText: string;
    technologiesUsed: string;
    description: string;
  }[];
}

const RECAPTCHA_SITE_KEY = import.meta.env.VITE_RECAPTCHA_SITE_KEY;

export default function CandidateRegistration() {
  const [resumeFile, setResumeFile] = useState<File | null>(null);
  const [resumeError, setResumeError] = useState<string | null>(null);
  const [recaptchaToken, setRecaptchaToken] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const [isSubmitted, setIsSubmitted] = useState(false);

  const {
    register,
    control,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    mode: "onBlur",
    defaultValues: {
      fullName: "",
      email: "",
      phone: "",
      currentLocation: "",
      linkedInUrl: "",
      skills: "",
      experience: [],
      education: [],
      projects: [],
    },
  });

  const experienceArray = useFieldArray({ control, name: "experience" });
  const educationArray = useFieldArray({ control, name: "education" });
  const projectsArray = useFieldArray({ control, name: "projects" });

  const handleResumeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    setResumeError(null);

    if (file) {
      const extension = file.name.slice(file.name.lastIndexOf(".")).toLowerCase();
      if (![".pdf", ".doc", ".docx"].includes(extension)) {
        setResumeError("Please attach a PDF, DOC or DOCX file.");
        setResumeFile(null);
        event.target.value = "";
        return;
      }
      if (file.size > 50 * 1024 * 1024) {
        setResumeError("Resume is too large. Maximum allowed size is 50 MB.");
        setResumeFile(null);
        event.target.value = "";
        return;
      }
    }

    setResumeFile(file);
  };

  const onSubmit = async (values: FormValues) => {
    setServerError(null);

    if (!resumeFile) {
      setResumeError("Please attach your resume.");
      return;
    }

    if (!recaptchaToken) {
      setServerError("Please complete the CAPTCHA verification.");
      return;
    }

    const skills = values.skills
      .split(",")
      .map((s) => s.trim())
      .filter((s) => s.length > 0);

    const response = await candidateRegistrationService.register({
      fullName: values.fullName.trim(),
      email: values.email.trim(),
      phone: values.phone.trim(),
      currentLocation: values.currentLocation.trim() || undefined,
      linkedInUrl: values.linkedInUrl.trim() || undefined,
      skills,
      experience: values.experience
        .filter((e) => e.companyName.trim())
        .map((e) => ({
          companyName: e.companyName.trim(),
          jobTitle: e.jobTitle.trim(),
          employmentType: e.employmentType || undefined,
          startDate: e.startDate,
          endDate: e.isCurrent ? undefined : e.endDate || undefined,
          isCurrent: e.isCurrent,
          location: e.location.trim() || undefined,
          description: e.description.trim() || undefined,
        })),
      education: values.education
        .filter((e) => e.degree.trim())
        .map((e) => ({
          degree: e.degree.trim(),
          institution: e.institution.trim(),
          fieldOfStudy: e.fieldOfStudy.trim() || undefined,
          startYear: e.startYear ? Number(e.startYear) : undefined,
          endYear: e.endYear ? Number(e.endYear) : undefined,
          isExpected: e.isExpected,
          grade: e.grade.trim() || undefined,
        })),
      projects: values.projects
        .filter((p) => p.projectName.trim())
        .map((p) => ({
          projectName: p.projectName.trim(),
          role: p.role.trim() || undefined,
          durationText: p.durationText.trim() || undefined,
          technologiesUsed: p.technologiesUsed.trim() || undefined,
          description: p.description.trim() || undefined,
        })),
      recaptchaToken,
      resume: resumeFile,
    });

    if (!response.success) {
      setServerError(response.message || "Unable to submit your application. Please try again.");
      return;
    }

    setIsSubmitted(true);
  };

  return (
    <div className="candidate-registration-page">
      <main className="candidate-registration-content">
        <div className="candidate-registration-card">
          <ITMLogo height={32} className="mb-1" />

          {isSubmitted ? (
            <div className="candidate-registration-success">
              <i className="bi bi-check-circle-fill" aria-hidden="true" />
              <h2>Thank you for applying!</h2>
              <p>
                We've received your profile and resume. Our recruitment team will review your application and get in
                touch if there's a suitable opportunity for you.
              </p>
            </div>
          ) : (
            <>
              <h2 className="candidate-registration-card__heading">Join Our Talent Database</h2>
              <p className="candidate-registration-card__subtitle">
                Submit your profile and resume — no account required.
              </p>

              {serverError && (
                <div className="candidate-registration-alert" role="alert">
                  <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
                  <span>{serverError}</span>
                </div>
              )}

              <form onSubmit={handleSubmit(onSubmit)} noValidate>
                <div className="candidate-registration-field">
                  <label htmlFor="fullName">Full Name</label>
                  <input
                    id="fullName"
                    className={`form-control ${errors.fullName ? "is-invalid" : ""}`}
                    {...register("fullName", { required: "Full name is required." })}
                  />
                  {errors.fullName && <div className="invalid-feedback">{errors.fullName.message}</div>}
                </div>

                <div className="candidate-registration-field">
                  <label htmlFor="email">Email Address</label>
                  <input
                    id="email"
                    type="email"
                    className={`form-control ${errors.email ? "is-invalid" : ""}`}
                    {...register("email", {
                      required: "Email address is required.",
                      pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Enter a valid email address." },
                    })}
                  />
                  {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
                </div>

                <div className="candidate-registration-field">
                  <label htmlFor="phone">Mobile Number</label>
                  <input
                    id="phone"
                    className={`form-control ${errors.phone ? "is-invalid" : ""}`}
                    placeholder="+91 98765 43210"
                    {...register("phone", {
                      required: "Mobile number is required.",
                      pattern: { value: /^\+?[0-9\s\-()]{7,20}$/, message: "Enter a valid mobile number." },
                    })}
                  />
                  {errors.phone && <div className="invalid-feedback">{errors.phone.message}</div>}
                </div>

                <div className="candidate-registration-field">
                  <label htmlFor="currentLocation">Current Location</label>
                  <input id="currentLocation" className="form-control" {...register("currentLocation")} />
                </div>

                <div className="candidate-registration-field">
                  <label htmlFor="linkedInUrl">LinkedIn Profile (optional)</label>
                  <input
                    id="linkedInUrl"
                    className={`form-control ${errors.linkedInUrl ? "is-invalid" : ""}`}
                    placeholder="https://www.linkedin.com/in/your-profile"
                    {...register("linkedInUrl", {
                      pattern: { value: /^https?:\/\/.+/i, message: "Enter a valid URL." },
                    })}
                  />
                  {errors.linkedInUrl && <div className="invalid-feedback">{errors.linkedInUrl.message}</div>}
                </div>

                <div className="candidate-registration-field">
                  <label htmlFor="skills">Primary Skills</label>
                  <input
                    id="skills"
                    className={`form-control ${errors.skills ? "is-invalid" : ""}`}
                    placeholder="e.g. React, .NET, SQL Server"
                    {...register("skills", { required: "Please list at least one primary skill." })}
                  />
                  <div className="candidate-registration-hint">Separate multiple skills with commas.</div>
                  {errors.skills && <div className="invalid-feedback">{errors.skills.message}</div>}
                </div>

                <section className="candidate-form-section">
                  <div className="candidate-form-section__header">
                    <h2 className="candidate-form-section__title">Work Experience</h2>
                    <button
                      type="button"
                      className="candidate-form-add-btn"
                      onClick={() =>
                        experienceArray.append({
                          companyName: "",
                          jobTitle: "",
                          employmentType: "",
                          startDate: "",
                          endDate: "",
                          isCurrent: false,
                          location: "",
                          description: "",
                        })
                      }
                    >
                      <i className="bi bi-plus-lg" aria-hidden="true" />
                      Add Experience
                    </button>
                  </div>

                  {experienceArray.fields.length === 0 && (
                    <p className="candidate-form-empty">No work experience added yet.</p>
                  )}

                  {experienceArray.fields.map((field, index) => (
                    <div className="candidate-form-card" key={field.id}>
                      <div className="row g-2">
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Company name"
                            {...register(`experience.${index}.companyName` as const)}
                          />
                        </div>
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Job title"
                            {...register(`experience.${index}.jobTitle` as const)}
                          />
                        </div>
                        <div className="col-6 col-md-3">
                          <select className="form-select" {...register(`experience.${index}.employmentType` as const)}>
                            <option value="">Type</option>
                            {EMPLOYMENT_TYPE_OPTIONS.map((option) => (
                              <option key={option.value} value={option.value}>
                                {option.label}
                              </option>
                            ))}
                          </select>
                        </div>
                        <div className="col-6 col-md-3">
                          <input
                            type="date"
                            className="form-control"
                            {...register(`experience.${index}.startDate` as const)}
                          />
                        </div>
                        <div className="col-6 col-md-3">
                          <input
                            type="date"
                            className="form-control"
                            disabled={!!watch(`experience.${index}.isCurrent`)}
                            {...register(`experience.${index}.endDate` as const)}
                          />
                        </div>
                        <div className="col-6 col-md-3 d-flex align-items-center">
                          <div className="form-check">
                            <input
                              type="checkbox"
                              className="form-check-input"
                              id={`experience-current-${field.id}`}
                              {...register(`experience.${index}.isCurrent` as const)}
                            />
                            <label className="form-check-label" htmlFor={`experience-current-${field.id}`}>
                              Current role
                            </label>
                          </div>
                        </div>
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Location (optional)"
                            {...register(`experience.${index}.location` as const)}
                          />
                        </div>
                        <div className="col-12">
                          <textarea
                            className="form-control"
                            rows={2}
                            placeholder="Description (optional)"
                            {...register(`experience.${index}.description` as const)}
                          />
                        </div>
                      </div>
                      <button
                        type="button"
                        className="candidate-form-remove-btn candidate-form-remove-btn--card"
                        onClick={() => experienceArray.remove(index)}
                        aria-label="Remove experience"
                      >
                        <i className="bi bi-trash-fill" aria-hidden="true" />
                      </button>
                    </div>
                  ))}
                </section>

                <section className="candidate-form-section">
                  <div className="candidate-form-section__header">
                    <h2 className="candidate-form-section__title">Education</h2>
                    <button
                      type="button"
                      className="candidate-form-add-btn"
                      onClick={() =>
                        educationArray.append({
                          degree: "",
                          institution: "",
                          fieldOfStudy: "",
                          startYear: "",
                          endYear: "",
                          isExpected: false,
                          grade: "",
                        })
                      }
                    >
                      <i className="bi bi-plus-lg" aria-hidden="true" />
                      Add Education
                    </button>
                  </div>

                  {educationArray.fields.length === 0 && <p className="candidate-form-empty">No education added yet.</p>}

                  {educationArray.fields.map((field, index) => (
                    <div className="candidate-form-card" key={field.id}>
                      <div className="row g-2">
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Degree"
                            {...register(`education.${index}.degree` as const)}
                          />
                        </div>
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Institution"
                            {...register(`education.${index}.institution` as const)}
                          />
                        </div>
                        <div className="col-12 col-md-4">
                          <input
                            className="form-control"
                            placeholder="Field of study (optional)"
                            {...register(`education.${index}.fieldOfStudy` as const)}
                          />
                        </div>
                        <div className="col-6 col-md-2">
                          <input
                            type="number"
                            className="form-control"
                            placeholder="Start year"
                            {...register(`education.${index}.startYear` as const)}
                          />
                        </div>
                        <div className="col-6 col-md-2">
                          <input
                            type="number"
                            className="form-control"
                            placeholder="End year"
                            {...register(`education.${index}.endYear` as const)}
                          />
                        </div>
                        <div className="col-6 col-md-2">
                          <input
                            className="form-control"
                            placeholder="Grade (optional)"
                            {...register(`education.${index}.grade` as const)}
                          />
                        </div>
                        <div className="col-6 col-md-2 d-flex align-items-center">
                          <div className="form-check">
                            <input
                              type="checkbox"
                              className="form-check-input"
                              id={`education-expected-${field.id}`}
                              {...register(`education.${index}.isExpected` as const)}
                            />
                            <label className="form-check-label" htmlFor={`education-expected-${field.id}`}>
                              Expected
                            </label>
                          </div>
                        </div>
                      </div>
                      <button
                        type="button"
                        className="candidate-form-remove-btn candidate-form-remove-btn--card"
                        onClick={() => educationArray.remove(index)}
                        aria-label="Remove education"
                      >
                        <i className="bi bi-trash-fill" aria-hidden="true" />
                      </button>
                    </div>
                  ))}
                </section>

                <section className="candidate-form-section">
                  <div className="candidate-form-section__header">
                    <h2 className="candidate-form-section__title">Projects</h2>
                    <button
                      type="button"
                      className="candidate-form-add-btn"
                      onClick={() =>
                        projectsArray.append({
                          projectName: "",
                          role: "",
                          durationText: "",
                          technologiesUsed: "",
                          description: "",
                        })
                      }
                    >
                      <i className="bi bi-plus-lg" aria-hidden="true" />
                      Add Project
                    </button>
                  </div>

                  {projectsArray.fields.length === 0 && <p className="candidate-form-empty">No projects added yet.</p>}

                  {projectsArray.fields.map((field, index) => (
                    <div className="candidate-form-card" key={field.id}>
                      <div className="row g-2">
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Project name"
                            {...register(`projects.${index}.projectName` as const)}
                          />
                        </div>
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Role (optional)"
                            {...register(`projects.${index}.role` as const)}
                          />
                        </div>
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Duration (optional, e.g. Jan 2022 - Jun 2022)"
                            {...register(`projects.${index}.durationText` as const)}
                          />
                        </div>
                        <div className="col-12 col-md-6">
                          <input
                            className="form-control"
                            placeholder="Technologies used (optional)"
                            {...register(`projects.${index}.technologiesUsed` as const)}
                          />
                        </div>
                        <div className="col-12">
                          <textarea
                            className="form-control"
                            rows={2}
                            placeholder="Description (optional)"
                            {...register(`projects.${index}.description` as const)}
                          />
                        </div>
                      </div>
                      <button
                        type="button"
                        className="candidate-form-remove-btn candidate-form-remove-btn--card"
                        onClick={() => projectsArray.remove(index)}
                        aria-label="Remove project"
                      >
                        <i className="bi bi-trash-fill" aria-hidden="true" />
                      </button>
                    </div>
                  ))}
                </section>

                <div className="candidate-registration-field">
                  <label htmlFor="resume">Resume</label>
                  <input
                    id="resume"
                    type="file"
                    accept=".pdf,.doc,.docx"
                    className={`form-control ${resumeError ? "is-invalid" : ""}`}
                    onChange={handleResumeChange}
                  />
                  <div className="candidate-registration-hint">PDF, DOC or DOCX — maximum 50 MB.</div>
                  {resumeError && <div className="invalid-feedback">{resumeError}</div>}
                </div>

                <div className="candidate-registration-field">
                  {RECAPTCHA_SITE_KEY ? (
                    <ReCAPTCHA sitekey={RECAPTCHA_SITE_KEY} onChange={(token) => setRecaptchaToken(token)} />
                  ) : (
                    <div className="candidate-registration-hint">
                      CAPTCHA is not configured for this environment.
                    </div>
                  )}
                </div>

                <button type="submit" className="candidate-registration-submit" disabled={isSubmitting} aria-busy={isSubmitting}>
                  {isSubmitting && <span className="candidate-registration-spinner" aria-hidden="true" />}
                  {isSubmitting ? "Submitting..." : "Submit Application"}
                </button>
              </form>

              <p className="candidate-registration-signin-link">
                Already have an account? <Link to="/login">Sign in</Link>
              </p>
            </>
          )}
        </div>
      </main>
    </div>
  );
}
