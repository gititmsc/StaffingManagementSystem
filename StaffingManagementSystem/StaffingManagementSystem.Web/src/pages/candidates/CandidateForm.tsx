import { useEffect, useState } from "react";
import { useFieldArray, useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import {
  CANDIDATE_SOURCE_OPTIONS,
  CANDIDATE_STATUS_OPTIONS,
  EMPLOYMENT_TYPE_OPTIONS,
  PROFICIENCY_OPTIONS,
} from "@/constants/candidates";
import { USER_MANAGEMENT_VIEW_ROLES } from "@/constants/roles";
import { candidatesService, type SaveCandidateRequest } from "@/services/candidatesService";
import { usersService, type ManagedUser } from "@/services/usersService";
import "./CandidateForm.css";

interface FormValues {
  fullName: string;
  email: string;
  phone: string;
  address: string;
  currentLocation: string;
  dateOfBirth: string;
  gender: string;
  status: string;
  source: string;
  ownerRecruiterId: string;
  skills: { skillName: string; proficiency: string; yearsOfExperience: string }[];
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

const EMPTY_FORM: FormValues = {
  fullName: "",
  email: "",
  phone: "",
  address: "",
  currentLocation: "",
  dateOfBirth: "",
  gender: "",
  status: CANDIDATE_STATUS_OPTIONS[0].value,
  source: "",
  ownerRecruiterId: "",
  skills: [],
  experience: [],
  education: [],
  projects: [],
};

function toDateInput(value?: string | null): string {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toISOString().slice(0, 10);
}

export default function CandidateForm() {
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const navigate = useNavigate();
  const { user: currentUser } = useAuth();
  const canReassignOwner = !!currentUser && USER_MANAGEMENT_VIEW_ROLES.includes(currentUser.role);

  const [isLoading, setIsLoading] = useState(isEdit);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [recruiters, setRecruiters] = useState<ManagedUser[]>([]);

  const {
    register,
    control,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ mode: "onBlur", defaultValues: EMPTY_FORM });

  const skillsArray = useFieldArray({ control, name: "skills" });
  const experienceArray = useFieldArray({ control, name: "experience" });
  const educationArray = useFieldArray({ control, name: "education" });
  const projectsArray = useFieldArray({ control, name: "projects" });

  useEffect(() => {
    if (canReassignOwner) {
      usersService.getAll().then((response) => {
        if (response.success && response.data) {
          setRecruiters(response.data.filter((u) => u.isActive));
        }
      });
    }
  }, [canReassignOwner]);

  useEffect(() => {
    if (!isEdit || !id) return;

    setIsLoading(true);
    setLoadError(null);

    candidatesService.getById(id).then((response) => {
      if (!response.success || !response.data) {
        setLoadError(response.message || "Unable to load this candidate.");
        setIsLoading(false);
        return;
      }

      const c = response.data;
      reset({
        fullName: c.fullName,
        email: c.email,
        phone: c.phone ?? "",
        address: c.address ?? "",
        currentLocation: c.currentLocation ?? "",
        dateOfBirth: toDateInput(c.dateOfBirth),
        gender: c.gender ?? "",
        status: c.status,
        source: c.source ?? "",
        ownerRecruiterId: c.ownerRecruiterId,
        skills: c.skills.map((s) => ({
          skillName: s.skillName,
          proficiency: s.proficiency ?? "",
          yearsOfExperience: s.yearsOfExperience != null ? String(s.yearsOfExperience) : "",
        })),
        experience: c.experience.map((e) => ({
          companyName: e.companyName,
          jobTitle: e.jobTitle,
          employmentType: e.employmentType ?? "",
          startDate: toDateInput(e.startDate),
          endDate: toDateInput(e.endDate),
          isCurrent: e.isCurrent,
          location: e.location ?? "",
          description: e.description ?? "",
        })),
        education: c.education.map((e) => ({
          degree: e.degree,
          institution: e.institution,
          fieldOfStudy: e.fieldOfStudy ?? "",
          startYear: e.startYear != null ? String(e.startYear) : "",
          endYear: e.endYear != null ? String(e.endYear) : "",
          isExpected: e.isExpected,
          grade: e.grade ?? "",
        })),
        projects: c.projects.map((p) => ({
          projectName: p.projectName,
          role: p.role ?? "",
          durationText: p.durationText ?? "",
          technologiesUsed: p.technologiesUsed ?? "",
          description: p.description ?? "",
        })),
      });
      setIsLoading(false);
    });
  }, [isEdit, id, reset]);

  const onSubmit = async (values: FormValues) => {
    setFormError(null);

    const payload: SaveCandidateRequest = {
      fullName: values.fullName.trim(),
      email: values.email.trim(),
      phone: values.phone.trim() || undefined,
      address: values.address.trim() || undefined,
      currentLocation: values.currentLocation.trim() || undefined,
      dateOfBirth: values.dateOfBirth || undefined,
      gender: values.gender.trim() || undefined,
      status: values.status,
      source: values.source || undefined,
      ownerRecruiterId: canReassignOwner && values.ownerRecruiterId ? values.ownerRecruiterId : undefined,
      skills: values.skills
        .filter((s) => s.skillName.trim())
        .map((s) => ({
          skillName: s.skillName.trim(),
          proficiency: s.proficiency || undefined,
          yearsOfExperience: s.yearsOfExperience ? Number(s.yearsOfExperience) : undefined,
        })),
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
    };

    const response = isEdit && id
      ? await candidatesService.update(id, payload)
      : await candidatesService.create(payload);

    if (!response.success || !response.data) {
      setFormError(response.message || "Unable to save this candidate. Please try again.");
      return;
    }

    navigate(`/candidates/${response.data.id}`);
  };

  if (isLoading) {
    return (
      <div className="container py-4">
        <div className="candidate-form-loading">
          <span
            className="login-spinner"
            style={{ borderTopColor: "var(--itm-primary)", borderColor: "rgba(22,58,99,0.2)" }}
          />
          <span>Loading candidate...</span>
        </div>
      </div>
    );
  }

  if (loadError) {
    return (
      <div className="container py-4">
        <div className="candidate-form-alert candidate-form-alert--error" role="alert">
          <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
          <span>{loadError}</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container py-4">
      <div className="candidate-form-header">
        <h1 className="h4 mb-1" style={{ color: "var(--itm-primary)" }}>
          {isEdit ? "Edit Candidate" : "Add Candidate"}
        </h1>
        <p className="text-muted mb-0">Capture personal details, skills, experience, education and projects.</p>
      </div>

      {formError && (
        <div className="candidate-form-alert candidate-form-alert--error" role="alert">
          <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
          <span>{formError}</span>
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <section className="candidate-form-section">
          <h2 className="candidate-form-section__title">Personal Details</h2>
          <div className="row g-3">
            <div className="col-12 col-md-6">
              <label className="candidate-form-label" htmlFor="fullName">
                Full Name
              </label>
              <input
                id="fullName"
                className={`form-control ${errors.fullName ? "is-invalid" : ""}`}
                {...register("fullName", { required: "Full name is required." })}
              />
              {errors.fullName && <div className="invalid-feedback">{errors.fullName.message}</div>}
            </div>

            <div className="col-12 col-md-6">
              <label className="candidate-form-label" htmlFor="email">
                Email Address
              </label>
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

            <div className="col-12 col-md-6">
              <label className="candidate-form-label" htmlFor="phone">
                Phone (optional)
              </label>
              <input id="phone" className="form-control" {...register("phone")} />
            </div>

            <div className="col-12 col-md-6">
              <label className="candidate-form-label" htmlFor="currentLocation">
                Current Location (optional)
              </label>
              <input id="currentLocation" className="form-control" {...register("currentLocation")} />
            </div>

            <div className="col-12">
              <label className="candidate-form-label" htmlFor="address">
                Address (optional)
              </label>
              <input id="address" className="form-control" {...register("address")} />
            </div>

            <div className="col-12 col-md-4">
              <label className="candidate-form-label" htmlFor="dateOfBirth">
                Date of Birth (optional)
              </label>
              <input id="dateOfBirth" type="date" className="form-control" {...register("dateOfBirth")} />
            </div>

            <div className="col-12 col-md-4">
              <label className="candidate-form-label" htmlFor="gender">
                Gender (optional)
              </label>
              <input id="gender" className="form-control" {...register("gender")} />
            </div>

            <div className="col-12 col-md-4">
              <label className="candidate-form-label" htmlFor="status">
                Status
              </label>
              <select id="status" className="form-select" {...register("status", { required: true })}>
                {CANDIDATE_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="col-12 col-md-6">
              <label className="candidate-form-label" htmlFor="source">
                Source (optional)
              </label>
              <select id="source" className="form-select" {...register("source")}>
                <option value="">Not specified</option>
                {CANDIDATE_SOURCE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            {canReassignOwner && (
              <div className="col-12 col-md-6">
                <label className="candidate-form-label" htmlFor="ownerRecruiterId">
                  Owner Recruiter
                </label>
                <select id="ownerRecruiterId" className="form-select" {...register("ownerRecruiterId")}>
                  <option value="">
                    {isEdit ? "Keep current owner" : "Assign to me"}
                  </option>
                  {recruiters.map((r) => (
                    <option key={r.id} value={r.id}>
                      {r.fullName}
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>
        </section>

        <section className="candidate-form-section">
          <div className="candidate-form-section__header">
            <h2 className="candidate-form-section__title">Skills</h2>
            <button
              type="button"
              className="candidate-form-add-btn"
              onClick={() => skillsArray.append({ skillName: "", proficiency: "", yearsOfExperience: "" })}
            >
              <i className="bi bi-plus-lg" aria-hidden="true" />
              Add Skill
            </button>
          </div>

          {skillsArray.fields.length === 0 && <p className="candidate-form-empty">No skills added yet.</p>}

          {skillsArray.fields.map((field, index) => (
            <div className="candidate-form-row" key={field.id}>
              <div className="row g-2 flex-grow-1">
                <div className="col-12 col-md-5">
                  <input
                    className="form-control"
                    placeholder="Skill name"
                    {...register(`skills.${index}.skillName` as const)}
                  />
                </div>
                <div className="col-6 col-md-4">
                  <select className="form-select" {...register(`skills.${index}.proficiency` as const)}>
                    <option value="">Proficiency</option>
                    {PROFICIENCY_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="col-6 col-md-3">
                  <input
                    type="number"
                    step="0.1"
                    min="0"
                    className="form-control"
                    placeholder="Years"
                    {...register(`skills.${index}.yearsOfExperience` as const)}
                  />
                </div>
              </div>
              <button
                type="button"
                className="candidate-form-remove-btn"
                onClick={() => skillsArray.remove(index)}
                aria-label="Remove skill"
              >
                <i className="bi bi-trash-fill" aria-hidden="true" />
              </button>
            </div>
          ))}
        </section>

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

          {experienceArray.fields.length === 0 && <p className="candidate-form-empty">No work experience added yet.</p>}

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

        <div className="candidate-form-actions">
          <button type="button" className="candidate-form-btn candidate-form-btn--ghost" onClick={() => navigate(-1)}>
            Cancel
          </button>
          <button type="submit" className="candidate-form-btn candidate-form-btn--primary" disabled={isSubmitting}>
            {isSubmitting && <span className="login-spinner" aria-hidden="true" />}
            {isEdit ? "Save Changes" : "Create Candidate"}
          </button>
        </div>
      </form>
    </div>
  );
}
