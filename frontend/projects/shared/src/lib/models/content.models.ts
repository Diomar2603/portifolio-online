export type PublishStatus = 'draft' | 'published';

export interface Media {
  id: string;
  key: string;
  mime: string;
  width: number;
  height: number;
  bytes: number;
  sha256: string;
  alt: string;
  status: 'pending' | 'ready';
  createdAt: string;
}

export interface Profile {
  name: string;
  headline: string;
  bio: string;
  links: Record<string, string>;
  avatarMediaId?: string | null;
}

export interface Education {
  id: string;
  institution: string;
  course: string;
  startDate: string;
  endDate?: string | null;
  description?: string;
  order: number;
}

export interface Experience {
  id: string;
  company: string;
  role: string;
  startDate: string;
  endDate?: string | null;
  description?: string;
  order: number;
}

export interface Certification {
  id: string;
  name: string;
  issuer: string;
  issuedAt: string;
  credentialUrl?: string;
  badgeMediaId?: string | null;
  order: number;
}

export interface Category {
  id: string;
  parentId: string | null;
  name: string;
  slug: string;
  order: number;
  icon?: string;
  color?: string;
  children?: Category[];
}

export interface ProjectRepo {
  url: string;
  language?: string;
  order: number;
}

export interface ProjectMedia {
  mediaId: string;
  order: number;
  caption?: string;
}

export interface Project {
  id: string;
  categoryId: string;
  title: string;
  slug: string;
  summary: string;
  /** Documento TipTap (JSON) */
  content: unknown;
  featured: boolean;
  status: PublishStatus;
  coverMediaId?: string | null;
  repos: ProjectRepo[];
  tags: string[];
  gallery: ProjectMedia[];
}

/** Conteúdo publicado consumido pelo build estático do site. */
export interface Snapshot {
  generatedAt: string;
  mediaBaseUrl: string;
  profile: Profile;
  education: Education[];
  experience: Experience[];
  certifications: Certification[];
  categories: Category[];
  projects: Project[];
  media: Media[];
}
