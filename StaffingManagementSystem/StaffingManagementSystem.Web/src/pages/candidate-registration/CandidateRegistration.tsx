import { useState } from "react";
import { useFieldArray, useForm } from "react-hook-form";
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
  const [experienceError, setExperienceError] = useState<string | null>(null);
  const [educationError, setEducationError] = useState<string | null>(null);
  const [projectsError, setProjectsError] = useState<string | null>(null);
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
    setExperienceError(null);
    setEducationError(null);
    setProjectsError(null);

    if (!resumeFile) {
      setResumeError("Please attach your resume.");
      return;
    }

    const experience = values.experience
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
      }));

    const education = values.education
      .filter((e) => e.degree.trim())
      .map((e) => ({
        degree: e.degree.trim(),
        institution: e.institution.trim(),
        fieldOfStudy: e.fieldOfStudy.trim() || undefined,
        startYear: e.startYear ? Number(e.startYear) : undefined,
        endYear: e.endYear ? Number(e.endYear) : undefined,
        isExpected: e.isExpected,
        grade: e.grade.trim() || undefined,
      }));

    const projects = values.projects
      .filter((p) => p.projectName.trim())
      .map((p) => ({
        projectName: p.projectName.trim(),
        role: p.role.trim() || undefined,
        durationText: p.durationText.trim() || undefined,
        technologiesUsed: p.technologiesUsed.trim() || undefined,
        description: p.description.trim() || undefined,
      }));

    let hasSectionError = false;
    if (experience.length === 0) {
      setExperienceError("Please add at least one work experience entry.");
      hasSectionError = true;
    }
    if (education.length === 0) {
      setEducationError("Please add at least one education entry.");
      hasSectionError = true;
    }
    if (projects.length === 0) {
      setProjectsError("Please add at least one project entry.");
      hasSectionError = true;
    }
    if (hasSectionError) {
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
      experience,
      education,
      projects,
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
                  <input
                    id="currentLocation"
                    className={`form-control ${errors.currentLocation ? "is-invalid" : ""}`}
                    {...register("currentLocation", { required: "Current location is required." })}
                  />
                  {errors.currentLocation && <div className="invalid-feedback">{errors.currentLocation.message}</div>}
                </div>

                <div className="candidate-registration-field">
                  <label htmlFor="linkedInUrl">LinkedIn Profile</label>
                  <input
                    id="linkedInUrl"
                    className={`form-control ${errors.linkedInUrl ? "is-invalid" : ""}`}
                    placeholder="https://www.linkedin.com/in/your-profile"
                    {...register("linkedInUrl", {
                      required: "LinkedIn profile URL is required.",
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
                    <h2 className="candidate-form-section__title">Work Experience *</h2>
                    <button
                      type="button"
                      className="candidate-form-add-btn"
                      onClick={() => {
                        setExperienceError(null);
                        experienceArray.append({
                          companyName: "",
                          jobTitle: "",
                          employmentType: "",
                          startDate: "",
                          endDate: "",
                          isCurrent: false,
                          location: "",
                          description: "",
                        });
                      }}
                    >
                      <i className="bi bi-plus-lg" aria-hidden="true" />
                      Add Experience
                    </button>
                  </div>

                  {experienceArray.fields.length === 0 && (
                    <p className="candidate-form-empty">Please add at least one work experience entry.</p>
                  )}
                  {experienceError && <div className="invalid-feedback d-block">{experienceError}</div>}

                  {experienceArray.fields.map((field, index) => {
                    const rowErrors = errors.experience?.[index];
                    const isCurrent = !!watch(`experience.${index}.isCurrent`);

                    return (
                      <div className="candidate-form-card" key={field.id}>
                        <div className="row g-2">
                          <div className="col-12 col-md-6">
                            <input
                              className={`form-control ${rowErrors?.companyName ? "is-invalid" : ""}`}
                              placeholder="Company name"
                              {...register(`experience.${index}.companyName` as const, {
                                required: "Company name is required.",
                              })}
                            />
                            {rowErrors?.companyName && (
                              <div className="invalid-feedback">{rowErrors.companyName.message}</div>
                            )}
                          </div>
                          <div className="col-12 col-md-6">
                            <input
                              className={`form-control ${rowErrors?.jobTitle ? "is-invalid" : ""}`}
                              placeholder="Job title"
                              {...register(`experience.${index}.jobTitle` as const, {
                                required: "Job title is required.",
                              })}
                            />
                            {rowErrors?.jobTitle && <div className="invalid-feedback">{rowErrors.jobTitle.message}</div>}
                          </div>
                          <div className="col-6 col-md-3">
                            <select
                              className={`form-select ${rowErrors?.employmentType ? "is-invalid" : ""}`}
                              {...register(`experience.${index}.employmentType` as const, {
                                required: "Type is required.",
                              })}
                            >
                              <option value="">Type</option>
                              {EMPLOYMENT_TYPE_OPTIONS.map((option) => (
                                <option key={option.value} value={option.value}>
                                  {option.label}
                                </option>
                              ))}
                            </select>
                            {rowErrors?.employmentType && (
                              <div className="invalid-feedback">{rowErrors.employmentType.message}</div>
                            )}
                          </div>
                          <div className="col-6 col-md-3">
                            <input
                              type="date"
                              className={`form-control ${rowErrors?.startDate ? "is-invalid" : ""}`}
                              {...register(`experience.${index}.startDate` as const, {
                                required: "Start date is required.",
                              })}
                            />
                            {rowErrors?.startDate && <div className="invalid-feedback">{rowErrors.startDate.message}</div>}
                          </div>
                          <div className="col-6 col-md-3">
                            <input
                              type="date"
                              className={`form-control ${rowErrors?.endDate ? "is-invalid" : ""}`}
                              disabled={isCurrent}
                              {...register(`experience.${index}.endDate` as const, {
                                validate: (value) => isCurrent || !!value || "End date is required.",
                              })}
                            />
                            {rowErrors?.endDate && <div className="invalid-feedback">{rowErrors.endDate.message}</div>}
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
                              className={`form-control ${rowErrors?.location ? "is-invalid" : ""}`}
                              placeholder="Location"
                              {...register(`experience.${index}.location` as const, {
                                required: "Location is required.",
                              })}
                            />
                            {rowErrors?.location && <div className="invalid-feedback">{rowErrors.location.message}</div>}
                          </div>
                          <div className="col-12">
                            <textarea
                              className={`form-control ${rowErrors?.description ? "is-invalid" : ""}`}
                              rows={2}
                              placeholder="Description"
                              {...register(`experience.${index}.description` as const, {
                                required: "Description is required.",
                              })}
                            />
                            {rowErrors?.description && (
                              <div className="invalid-feedback">{rowErrors.description.message}</div>
                            )}
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
                    );
                  })}
                </section>

                <section className="candidate-form-section">
                  <div className="candidate-form-section__header">
                    <h2 className="candidate-form-section__title">Education *</h2>
                    <button
                      type="button"
                      className="candidate-form-add-btn"
                      onClick={() => {
                        setEducationError(null);
                        educationArray.append({
                          degree: "",
                          institution: "",
                          fieldOfStudy: "",
                          startYear: "",
                          endYear: "",
                          isExpected: false,
                          grade: "",
                        });
                      }}
                    >
                      <i className="bi bi-plus-lg" aria-hidden="true" />
                      Add Education
                    </button>
                  </div>

                  {educationArray.fields.length === 0 && (
                    <p className="candidate-form-empty">Please add at least one education entry.</p>
                  )}
                  {educationError && <div className="invalid-feedback d-block">{educationError}</div>}

                  {educationArray.fields.map((field, index) => {
                    const rowErrors = errors.education?.[index];
                    const isExpected = !!watch(`education.${index}.isExpected`);

                    return (
                      <div className="candidate-form-card" key={field.id}>
                        <div className="row g-2">
                          <div className="col-12 col-md-6">
                            <input
                              className={`form-control ${rowErrors?.degree ? "is-invalid" : ""}`}
                              placeholder="Degree"
                              {...register(`education.${index}.degree` as const, {
                                required: "Degree is required.",
                              })}
                            />
                            {rowErrors?.degree && <div className="invalid-feedback">{rowErrors.degree.message}</div>}
                          </div>
                          <div className="col-12 col-md-6">
                            <input
                              className={`form-control ${rowErrors?.institution ? "is-invalid" : ""}`}
                              placeholder="Institution"
                              {...register(`education.${index}.institution` as const, {
                                required: "Institution is required.",
                              })}
                            />
                            {rowErrors?.institution && (
                              <div className="invalid-feedback">{rowErrors.institution.message}</div>
                            )}
                          </div>
                          <div className="col-12 col-md-4">
                            <input
                              className={`form-control ${rowErrors?.fieldOfStudy ? "is-invalid" : ""}`}
                              placeholder="Field of study"
                              {...register(`education.${index}.fieldOfStudy` as const, {
                                required: "Field of study is required.",
                              })}
                            />
                            {rowErrors?.fieldOfStudy && (
                              <div className="invalid-feedback">{rowErrors.fieldOfStudy.message}</div>
                            )}
                          </div>
                          <div className="col-6 col-md-2">
                            <input
                              type="number"
                              className={`form-control ${rowErrors?.startYear ? "is-invalid" : ""}`}
                              placeholder="Start year"
                              {...register(`education.${index}.startYear` as const, {
                                required: "Required.",
                              })}
                            />
                            {rowErrors?.startYear && <div className="invalid-feedback">{rowErrors.startYear.message}</div>}
                          </div>
                          <div className="col-6 col-md-2">
                            <input
                              type="number"
                              className={`form-control ${rowErrors?.endYear ? "is-invalid" : ""}`}
                              placeholder="End year"
                              disabled={isExpected}
                              {...register(`education.${index}.endYear` as const, {
                                validate: (value) => isExpected || !!value || "Required.",
                              })}
                            />
                            {rowErrors?.endYear && <div className="invalid-feedback">{rowErrors.endYear.message}</div>}
                          </div>
                          <div className="col-6 col-md-2">
                            <input
                              className={`form-control ${rowErrors?.grade ? "is-invalid" : ""}`}
                              placeholder="Grade"
                              {...register(`education.${index}.grade` as const, {
                                required: "Required.",
                              })}
                            />
                            {rowErrors?.grade && <div className="invalid-feedback">{rowErrors.grade.message}</div>}
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
                    );
                  })}
                </section>

                <section className="candidate-form-section">
                  <div className="candidate-form-section__header">
                    <h2 className="candidate-form-section__title">Projects *</h2>
                    <button
                      type="button"
                      className="candidate-form-add-btn"
                      onClick={() => {
                        setProjectsError(null);
                        projectsArray.append({
                          projectName: "",
                          role: "",
                          durationText: "",
                          technologiesUsed: "",
                          description: "",
                        });
                      }}
                    >
                      <i className="bi bi-plus-lg" aria-hidden="true" />
                      Add Project
                    </button>
                  </div>

                  {projectsArray.fields.length === 0 && (
                    <p className="candidate-form-empty">Please add at least one project entry.</p>
                  )}
                  {projectsError && <div className="invalid-feedback d-block">{projectsError}</div>}

                  {projectsArray.fields.map((field, index) => {
                    const rowErrors = errors.projects?.[index];

                    return (
                      <div className="candidate-form-card" key={field.id}>
                        <div className="row g-2">
                          <div className="col-12 col-md-6">
                            <input
                              className={`form-control ${rowErrors?.projectName ? "is-invalid" : ""}`}
                              placeholder="Project name"
                              {...register(`projects.${index}.projectName` as const, {
                                required: "Project name is required.",
                              })}
                            />
                            {rowErrors?.projectName && (
                              <div className="invalid-feedback">{rowErrors.projectName.message}</div>
                            )}
                          </div>
                          <div className="col-12 col-md-6">
                            <input
                              className={`form-control ${rowErrors?.role ? "is-invalid" : ""}`}
                              placeholder="Role"
                              {...register(`projects.${index}.role` as const, { required: "Role is required." })}
                            />
                            {rowErrors?.role && <div className="invalid-feedback">{rowErrors.role.message}</div>}
                          </div>
                          <div className="col-12 col-md-6">
                            <input
                              className={`form-control ${rowErrors?.durationText ? "is-invalid" : ""}`}
                              placeholder="Duration (e.g. Jan 2022 - Jun 2022)"
                              {...register(`projects.${index}.durationText` as const, {
                                required: "Duration is required.",
                              })}
                            />
                            {rowErrors?.durationText && (
                              <div className="invalid-feedback">{rowErrors.durationText.message}</div>
                            )}
                          </div>
                          <div className="col-12 col-md-6">
                            <input
                              className={`form-control ${rowErrors?.technologiesUsed ? "is-invalid" : ""}`}
                              placeholder="Technologies used"
                              {...register(`projects.${index}.technologiesUsed` as const, {
                                required: "Technologies used is required.",
                              })}
                            />
                            {rowErrors?.technologiesUsed && (
                              <div className="invalid-feedback">{rowErrors.technologiesUsed.message}</div>
                            )}
                          </div>
                          <div className="col-12">
                            <textarea
                              className={`form-control ${rowErrors?.description ? "is-invalid" : ""}`}
                              rows={2}
                              placeholder="Description"
                              {...register(`projects.${index}.description` as const, {
                                required: "Description is required.",
                              })}
                            />
                            {rowErrors?.description && (
                              <div className="invalid-feedback">{rowErrors.description.message}</div>
                            )}
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
                    );
                  })}
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
            </>
          )}
        </div>
      </main>
    </div>
  );
}
